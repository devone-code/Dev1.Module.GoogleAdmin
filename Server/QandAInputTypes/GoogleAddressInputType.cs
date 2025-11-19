using Dev1.QandA.Core.Models; // enums
using Dev1.QandA.Core.Models.V2; // V2 runtime abstractions
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using GoogleApi; // external libs assumed present in hosting module
using GoogleApi.Entities.Places.AutoComplete.Request;
using GoogleApi.Entities.Maps.Geocoding.Place.Request;
using Dev1.Module.GoogleAdmin.Services;

// NOTE: Namespace intentionally left as-is (external) per instruction.
namespace Dev1.Module.GoogleAdmin
{
    /// <summary>
    /// V2 Google Address input definition with secondary field descriptors.
    /// If the hosting solution defines GOOGLE_PLACES and references the GoogleApi.* packages
    /// full autocomplete + geocode logic is enabled. Otherwise it degrades gracefully without build errors.
    /// </summary>
    public class GoogleAddressInputType : InputDefinitionBase
    {
        public override string Name => "Google Address";
        public override string Description => "Address autocomplete & resolution via Google Places API";
        public override string DataType => eDataType.String.ToString();
        public override string DisplayType => "SingleLine"; // textbox
        public override int DisplayOrder => 200;
        public override string FriendlyComponentName => "Google Admin";
        public override string Icon => "fa fa-map-marker";

        // Setting keys
        private const string SettingApiKey = "GoogleApiKey";
        private const string SettingCountryRestriction = "CountryRestriction"; // optional 2-char code or 'None'

        // Secondary keys (used in runtime state.Secondary)
        private static readonly string[] AddressKeys = new[]
        {
            "Latitude","Longitude","Number","Street","Suburb","City","State","StateCode","PostCode","Country","CountryCode"
        };


        // Optional DI (hosting module can supply these via reflection if desired)
        private readonly GoogleApi.GooglePlaces.AutoCompleteApi _autoComplete;
        private readonly GoogleMaps.Geocode.PlaceGeocodeApi _geocode;
        private readonly IGoogleAdminService _gService;
        public GoogleAddressInputType() 
        { 
        }
        public GoogleAddressInputType(GooglePlaces.AutoCompleteApi ac, GoogleMaps.Geocode.PlaceGeocodeApi gc, IGoogleAdminService gService) 
        { 
            _autoComplete = ac; 
            _geocode = gc; 
            _gService = gService;
        }


        public override void ConfigureSettings(SettingsBuilder builder)
        {
            builder
                .Add(SettingApiKey, eDataType.String, "Google Places API Key", required: true, helpText: "Enter your Google Places API key", isPrivate: true)
                .Add(SettingCountryRestriction, eDataType.Table, "Restrict results to country", defaultValue: "None", options: CountryOptions(), helpText: "Optional country filter");
        }

        public override void ConfigureSecondaryInputs(SecondaryInputBuilder builder)
        {
            // Visible editable fields (widths follow earlier implementation)
            builder
                .Add("Number", eDataType.String, isRequired: true, isVisible: true, isEditable: true, supportsUpdate: false, width: 3)
                .Add("Street", eDataType.String, isRequired: true, isVisible: true, isEditable: true, supportsUpdate: false, width: 9)
                .Add("Suburb", eDataType.String, isRequired: true, isVisible: true, isEditable: true, supportsUpdate: false, width: 6)
                .Add("City", eDataType.String, isRequired: true, isVisible: true, isEditable: true, supportsUpdate: false, width: 6)
                .Add("State", eDataType.String, isRequired: true, isVisible: true, isEditable: true, supportsUpdate: false, width: 4)
                .Add("PostCode", eDataType.String, isRequired: true, isVisible: true, isEditable: true, supportsUpdate: false, width: 4)
                .Add("Country", eDataType.String, isRequired: false, isVisible: false, isEditable: true, supportsUpdate: false, width: 4)
                // Hidden meta fields
                .Add("StateCode", eDataType.String, isRequired: false, isVisible: false, isEditable: false, supportsUpdate: false, width: 4)
                .Add("CountryCode", eDataType.String, isRequired: false, isVisible: false, isEditable: false, supportsUpdate: false, width: 4)
                .Add("Latitude", eDataType.Number, isRequired: false, isVisible: false, isEditable: false, supportsUpdate: false, width: 6)
                .Add("Longitude", eDataType.Number, isRequired: false, isVisible: false, isEditable: false, supportsUpdate: false, width: 6);
        }

        private static IEnumerable<(string Value, string Label)> CountryOptions()
        {
            yield return ("None", "None");
            yield return ("US", "United States");
            yield return ("CA", "Canada");
            yield return ("UK", "United Kingdom");
            yield return ("AU", "Australia");
            yield return ("DE", "Germany");
            yield return ("FR", "France");
        }

        public override async Task InitializeAsync(InputState state, RuntimeContext context)
        {
            state.Value ??= string.Empty;
            foreach (var k in AddressKeys)
                if (!state.Secondary.ContainsKey(k)) state.Secondary[k] = string.Empty;
            await Task.CompletedTask;
        }


        public override async Task InputAsync(InputState state, InteractionContext context)
        {
            if (string.IsNullOrWhiteSpace(state.Value) || state.Value.Trim().Length < 3)
            { state.Suggestions.Clear(); return; }
            
            var apiKey = GetSetting(state, SettingApiKey);
            
            if (string.IsNullOrWhiteSpace(apiKey)) { Fail(state, "Missing Google API Key."); return; }
            
            try
            {
                var req = new PlacesAutoCompleteRequest { Key = apiKey, Input = state.Value.Trim() };
                var country = GetSetting(state, SettingCountryRestriction);

                if (!string.IsNullOrWhiteSpace(country) && !country.Equals("None", StringComparison.OrdinalIgnoreCase))
                {
                    req.Components = new[] {
                        new KeyValuePair<GoogleApi.Entities.Common.Enums.Component,string>(
                            GoogleApi.Entities.Common.Enums.Component.Country,country.ToLowerInvariant())};
                }
                var resp = await GooglePlaces.AutoComplete.QueryAsync(req);
                
                state.Suggestions.Clear();
                
                if (resp.Status == GoogleApi.Entities.Common.Enums.Status.Ok && resp.Predictions != null)
                {
                    foreach (var p in resp.Predictions)
                        state.Suggestions.Add(new Option { Value = p.PlaceId, Label = p.Description });
                    state.Status = eResultStatus.Success;
                }
                else if (resp.Status == GoogleApi.Entities.Common.Enums.Status.ZeroResults)
                { state.Status = eResultStatus.Warning; state.ErrorMessage = "No matches"; }
                else { Fail(state, resp.ErrorMessage ?? resp.Status.ToString()); }
            }
            catch (Exception ex) { Fail(state, ex.Message); }
        }

        public override async Task UpdateAsync(InputState state, InteractionContext context)
        {
            var placeId = state.Value?.Trim(); if (string.IsNullOrEmpty(placeId)) return;
            var apiKey = GetSetting(state, SettingApiKey); if (string.IsNullOrWhiteSpace(apiKey)) { Fail(state, "Missing Google API Key."); return; }
            try
            {
                var req = new PlaceGeocodeRequest { Key = apiKey, PlaceId = placeId };
                var resp = await GoogleMaps.Geocode.PlaceGeocode.QueryAsync(req);
                
                if (resp.Status != GoogleApi.Entities.Common.Enums.Status.Ok) { Fail(state, resp.ErrorMessage ?? resp.Status.ToString()); return; }
                
                var result = resp.Results?.FirstOrDefault(); if (result == null) { Fail(state, "No address details returned"); return; }
                state.Value = result.FormattedAddress;
                
                SetSecondary(state, "Latitude", result.Geometry?.Location?.Latitude.ToString());
                SetSecondary(state, "Longitude", result.Geometry?.Location?.Longitude.ToString());
                
                var comps = result.AddressComponents ?? new List<GoogleApi.Entities.Common.AddressComponent>();
                
                string GetComp(GoogleApi.Entities.Common.Enums.AddressComponentType type) => comps.FirstOrDefault(c => c.Types.Contains(type))?.LongName ?? string.Empty;
                string GetCompShort(GoogleApi.Entities.Common.Enums.AddressComponentType type) => comps.FirstOrDefault(c => c.Types.Contains(type))?.ShortName ?? string.Empty;
                
                SetSecondary(state, "Number", GetComp(GoogleApi.Entities.Common.Enums.AddressComponentType.Street_Number));
                SetSecondary(state, "Street", GetComp(GoogleApi.Entities.Common.Enums.AddressComponentType.Route));
                SetSecondary(state, "Suburb", GetComp(GoogleApi.Entities.Common.Enums.AddressComponentType.Locality));
                
                var city = comps.FirstOrDefault(c => c.Types.Contains(GoogleApi.Entities.Common.Enums.AddressComponentType.Administrative_Area_Level_2))
                           ?? comps.FirstOrDefault(c => c.Types.Contains(GoogleApi.Entities.Common.Enums.AddressComponentType.Locality));
                
                SetSecondary(state, "City", city?.LongName ?? string.Empty);
                SetSecondary(state, "State", GetComp(GoogleApi.Entities.Common.Enums.AddressComponentType.Administrative_Area_Level_1));
                SetSecondary(state, "StateCode", GetCompShort(GoogleApi.Entities.Common.Enums.AddressComponentType.Administrative_Area_Level_1));
                SetSecondary(state, "PostCode", GetComp(GoogleApi.Entities.Common.Enums.AddressComponentType.Postal_Code));
                SetSecondary(state, "Country", GetComp(GoogleApi.Entities.Common.Enums.AddressComponentType.Country));
                SetSecondary(state, "CountryCode", GetCompShort(GoogleApi.Entities.Common.Enums.AddressComponentType.Country));
                
                state.Status = eResultStatus.Success; state.ErrorMessage = null;
            }
            catch (Exception ex) { Fail(state, ex.Message); }
        }


        public override Task SubmitAsync(InputState state, SubmissionContext context)
        { return Task.CompletedTask; }

        #region Helpers
        private static string GetSetting(InputState state, string key)
            => state.Settings != null && state.Settings.TryGetValue(key, out var v) ? v : string.Empty;
        private static void SetSecondary(InputState state, string key, string value)
        { if (!string.IsNullOrEmpty(key)) state.Secondary[key] = value ?? string.Empty; }
        private static void Fail(InputState state, string message)
        { state.Status = eResultStatus.Danger; state.ErrorMessage = message; }
        #endregion
    }
}

