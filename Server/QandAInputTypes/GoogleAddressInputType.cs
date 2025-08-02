using Dev1.Flow.Core;
using Dev1.Flow.Core.Models;
using Dev1.QandA.Core.Interfaces;
using Dev1.QandA.Core.Models;
using GoogleApi;
using GoogleApi.Entities.Maps.Geocoding.Place.Request;
using GoogleApi.Entities.Places.AutoComplete.Request;
using GoogleApi.Entities.Maps.Geocoding.Address.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev1.Module.GoogleAdmin
{
    public class GoogleAddressInputType : IQandAInputDefinition
    {

        private readonly GoogleApi.GooglePlaces.AutoCompleteApi _autoComplete; 

        private readonly GoogleMaps.Geocode.PlaceGeocodeApi _geocode;

        public GoogleAddressInputType(GoogleApi.GooglePlaces.AutoCompleteApi autoComplete, GoogleMaps.Geocode.PlaceGeocodeApi geocode   )
        {
            _autoComplete = autoComplete ?? throw new ArgumentNullException(nameof(autoComplete));
            _geocode = geocode;
        }

        // Core Identity
        public string Key => this.GetType().FullName;

        //Your Input types will be grouled by this name in the designer.
        public string FriendlyComponentName => "Google Admin";
        public string Name => "Google Address Autocomplete";
        public string Description => "Provides address autocomplete using Google Places API";
        public string Icon => "fa fa-map-marker";
        public int DisplayOrder => 1;


        public bool IsVisibleInDesigner => true;
        public bool SupportsInput => true;   // Needs real-time input for autocomplete
        public bool SupportsUpdate => true;  // Needs selection events
        public bool SupportsExecute => true; // Needs post-submission processing
        
        // Custom input specific
        public MainInputProperty MainInputProperty { get; set; }
        public IList<CustomInputSetting> CustomInputSettings { get; set; }
        public IList<SecondaryInputProperty> SecondaryInputProperties { get; set; }
        public Data Data { get; set; }



        //You can define your secondary input properties and setttins as enums if you wish
        private enum  eSecondaryProperties
        {
            Latitude,
            Longitude,
            StreetNumber,
            Street,
            Suburb,
            City,
            State,
            PostalCode,
            Country
        }

        private enum eSettings
        {
            GoogleApiKey,
            CountryRestriction
        }
        public GoogleAddressInputType()
        {
            MainInputProperty = new MainInputProperty
            {
                Name = "Address",
                //Description = "Enter an address",
                DataType = eDataType.String,
                DisplayType = StringDisplayType.Text.ToString(),
                //HelpText = "Type an address to get suggestions",
            };
            
            SecondaryInputProperties = new List<SecondaryInputProperty>
            {
                new SecondaryInputProperty
                {
                    Name = eSecondaryProperties.StreetNumber.ToString(),
                    HelpText = "",
                    IsVisible = true,
                    IsEditable = true,
                    DataType = eDataType.String,
                    DisplayType = StringDisplayType.Text.ToString(),
                    SupportsUpdate = true, 
                    Width = 3,
                },
                new SecondaryInputProperty
                {
                    Name = eSecondaryProperties.Street.ToString(),
                    HelpText = "",
                    IsVisible = true,
                    IsEditable = true,
                    DataType = eDataType.String,
                    DisplayType = StringDisplayType.Text.ToString(),
                    SupportsUpdate = true, 
                    Width = 9,
                },
                new SecondaryInputProperty
                {
                    Name = eSecondaryProperties.Suburb.ToString(),
                    HelpText = "",
                    IsVisible = true,
                    IsEditable = true,
                    DataType = eDataType.String,
                    DisplayType = StringDisplayType.Text.ToString(),
                    SupportsUpdate = true, 
                    Width = 6,
                },
                new SecondaryInputProperty
                {
                    Name = eSecondaryProperties.City.ToString(),
                    HelpText = "",
                    IsVisible = true,
                    IsEditable = true,
                    DataType = eDataType.String,
                    DisplayType = StringDisplayType.Text.ToString(),
                    SupportsUpdate = true, 
                    Width = 6,
                },
                new SecondaryInputProperty
                {
                    Name = eSecondaryProperties.State.ToString(),
                    HelpText = "",
                    IsVisible = true,
                    IsEditable = true,
                    DataType = eDataType.String,
                    DisplayType = StringDisplayType.Text.ToString(),
                    SupportsUpdate = true, 
                    Width = 4,
                },
                new SecondaryInputProperty
                {
                    Name = eSecondaryProperties.PostalCode.ToString(),
                    HelpText = "",
                    IsVisible = true,
                    IsEditable = true,
                    DataType = eDataType.String,
                    DisplayType = StringDisplayType.Text.ToString(),
                    SupportsUpdate = true, 
                    Width = 4,
                },
                new SecondaryInputProperty
                {
                    Name = eSecondaryProperties.Country.ToString(),
                    HelpText = "",
                    IsVisible = true,
                    IsEditable = true,
                    DataType = eDataType.String,
                    DisplayType = StringDisplayType.Text.ToString(),
                    SupportsUpdate = true, 
                    Width = 4,
                },
                new SecondaryInputProperty
                {
                    Name = eSecondaryProperties.Latitude.ToString(),
                    HelpText = "",
                    IsVisible = false,
                    IsEditable = true,
                    DataType = eDataType.Number,
                    DisplayType = DisplayTypes.Number.WholeNumber,
                    SupportsUpdate = true, 
                    Width = 6,
                },
                new SecondaryInputProperty
                {
                    Name = eSecondaryProperties.Longitude.ToString(),
                    HelpText = "",
                    IsVisible = false,
                    IsEditable = true,
                    SupportsUpdate = true,
                    DataType = eDataType.Number,
                    DisplayType = DisplayTypes.Number.Slider,
                    Width = 6
                }
            };

            CustomInputSettings = new List<CustomInputSetting>
            {
                new CustomInputSetting
                {
                    Name = eSettings.GoogleApiKey.ToString(),
                    Description = "Your Google API Key",
                    DataType = eDataType.String,
                    IsRequired = true,
                    IsPrivate = true,
                    HelpText = "Enter your Google Places API key"
                },
                new CustomInputSetting
                {
                    Name = eSettings.CountryRestriction.ToString(),
                    Description = "Restrict results to specific country",
                    DataType = eDataType.Table,
                    HelpText = "Select country to restrict address results",
                    DisplayOptionValue = "US",
                    Options = new[] { "US", "CA", "UK", "AU", "DE", "FR" }
                },
            };
        }

        // Event handlers remain unchanged
        public async Task<CustomInputTransfer> OnInputAsync(CustomInputTransfer customInput, int moduleId, int userId, int siteId, int QuestionId)
        {
            // ... existing implementation unchanged
            try
            {
                var request = new PlacesAutoCompleteRequest
                {
                    Key = customInput.Settings?.FirstOrDefault(x => x.Name == eSettings.GoogleApiKey.ToString())?.Value?.ToString() ?? "",
                    Input = customInput.MainInputProperty?.Value?.ToString() ?? "",
                    Region = customInput.Settings?.FirstOrDefault(x => x.Name == eSettings.CountryRestriction.ToString())?.Value?.ToString() ?? "",
                };

                var response = await GooglePlaces.AutoComplete.QueryAsync(request);
                var suggestions = new List<TransferDataItem>();

                if (response.Status == GoogleApi.Entities.Common.Enums.Status.Ok)
                {
                    if (response?.Predictions != null && response.Predictions.Any())
                    {
                        foreach (var prediction in response.Predictions)
                        {
                            suggestions.Add(new TransferDataItem
                            {
                                Name = prediction.Description,
                                Value = prediction.PlaceId // Use PlaceId as value for selection
                            });
                        }
                    }

                    customInput.Data = new TransferData
                    {
                        Items = suggestions,
                        //SelectedItem = suggestions.FirstOrDefault() // Automatically select the first suggestion
                    };

                    customInput.Status = eResultStatus.Success;
                    customInput.ErrorMessage = "";
                }
                else
                {
                    customInput.Status = eResultStatus.Danger;
                    customInput.ErrorMessage = response.ErrorMessage;
                }

                    return customInput;

            }
            catch (Exception ex)
            {
                customInput.Status = eResultStatus.Danger;
                customInput.ErrorMessage = $"Error processing address input: {ex.Message}";
                foreach (var sec in customInput.SecondaryProperties)
                {
                    sec.IsVisible = false;
                }
                return customInput;
            }
        }

        public async Task<CustomInputTransfer> OnUpdateAsync(CustomInputTransfer customInput, int moduleId, int userId, int siteId, int QuestionId)
        {
            try
            {
                // Check if this update was triggered by a secondary property
                var updatedSecondaryProperty = customInput.SecondaryProperties?.FirstOrDefault(x => x.IsForUpdate);
                
                if (updatedSecondaryProperty != null)
                {
                    // Secondary property was updated - reconstruct the formatted address from components
                    var streetNumber = customInput.SecondaryProperties.FirstOrDefault(x => x.Name == eSecondaryProperties.StreetNumber.ToString())?.Value?.ToString() ?? "";
                    var street = customInput.SecondaryProperties.FirstOrDefault(x => x.Name == eSecondaryProperties.Street.ToString())?.Value?.ToString() ?? "";
                    var suburb = customInput.SecondaryProperties.FirstOrDefault(x => x.Name == eSecondaryProperties.Suburb.ToString())?.Value?.ToString() ?? "";
                    var city = customInput.SecondaryProperties.FirstOrDefault(x => x.Name == eSecondaryProperties.City.ToString())?.Value?.ToString() ?? "";
                    var state = customInput.SecondaryProperties.FirstOrDefault(x => x.Name == eSecondaryProperties.State.ToString())?.Value?.ToString() ?? "";
                    var postalCode = customInput.SecondaryProperties.FirstOrDefault(x => x.Name == eSecondaryProperties.PostalCode.ToString())?.Value?.ToString() ?? "";
                    var country = customInput.SecondaryProperties.FirstOrDefault(x => x.Name == eSecondaryProperties.Country.ToString())?.Value?.ToString() ?? "";

                    // Build formatted address from components
                    var addressParts = new List<string>();
                    
                    // Street address (number + street)
                    var streetAddress = $"{streetNumber} {street}".Trim();
                    if (!string.IsNullOrWhiteSpace(streetAddress))
                        addressParts.Add(streetAddress);
                    
                    // Add locality components
                    if (!string.IsNullOrWhiteSpace(suburb))
                        addressParts.Add(suburb);
                    if (!string.IsNullOrWhiteSpace(city))
                        addressParts.Add(city);
                    if (!string.IsNullOrWhiteSpace(state))
                        addressParts.Add(state);
                    if (!string.IsNullOrWhiteSpace(postalCode))
                        addressParts.Add(postalCode);
                    if (!string.IsNullOrWhiteSpace(country))
                        addressParts.Add(country);

                    // Update the main input property with the reconstructed address
                    customInput.MainInputProperty.Value = string.Join(", ", addressParts);

                    // Reset the IsForUpdate flag
                    updatedSecondaryProperty.IsForUpdate = false;

                    customInput.Status = eResultStatus.Success;
                    customInput.ErrorMessage = "";
                }
                else
                {
                    // Main input was updated (PlaceId from autocomplete selection) - existing logic
                    var request = new PlaceGeocodeRequest
                    {
                        Key = customInput.Settings?.FirstOrDefault(x => x.Name == eSettings.GoogleApiKey.ToString())?.Value?.ToString() ?? "",
                        PlaceId = customInput.MainInputProperty.Value?.ToString() ?? "",
                    };

                    if (!String.IsNullOrEmpty(request.PlaceId))
                    {

                        var response = await GoogleMaps.Geocode.PlaceGeocode.QueryAsync(request);

                        customInput.Data = null;

                        if (response.Status == GoogleApi.Entities.Common.Enums.Status.Ok)
                        {
                            var result = response.Results.FirstOrDefault();

                            if (result != null)
                            {
                                // Set the formatted address
                                customInput.MainInputProperty.Value = result.FormattedAddress;

                                // Set coordinates (hidden fields)
                                customInput.SecondaryProperties.First(x => x.Name == eSecondaryProperties.Latitude.ToString()).Value = result.Geometry.Location.Latitude.ToString();
                                customInput.SecondaryProperties.First(x => x.Name == eSecondaryProperties.Longitude.ToString()).Value = result.Geometry.Location.Longitude.ToString();

                                // Extract address components
                                var addressComponents = result.AddressComponents;

                                if (addressComponents != null)
                                {
                                    // Street Number
                                    var streetNumber = addressComponents.FirstOrDefault(x => x.Types.Contains(GoogleApi.Entities.Common.Enums.AddressComponentType.Street_Number));
                                    customInput.SecondaryProperties.First(x => x.Name == eSecondaryProperties.StreetNumber.ToString()).Value = streetNumber?.LongName ?? "";

                                    // Street/Route
                                    var route = addressComponents.FirstOrDefault(x => x.Types.Contains(GoogleApi.Entities.Common.Enums.AddressComponentType.Route));
                                    customInput.SecondaryProperties.First(x => x.Name == eSecondaryProperties.Street.ToString()).Value = route?.LongName ?? "";

                                    // Suburb/Locality
                                    var locality = addressComponents.FirstOrDefault(x => x.Types.Contains(GoogleApi.Entities.Common.Enums.AddressComponentType.Locality));
                                    customInput.SecondaryProperties.First(x => x.Name == eSecondaryProperties.Suburb.ToString()).Value = locality?.LongName ?? "";

                                    // City (Administrative Area Level 2 or Locality if no Level 2)
                                    var city = addressComponents.FirstOrDefault(x => x.Types.Contains(GoogleApi.Entities.Common.Enums.AddressComponentType.Administrative_Area_Level_2)) ??
                                              addressComponents.FirstOrDefault(x => x.Types.Contains(GoogleApi.Entities.Common.Enums.AddressComponentType.Locality));
                                    customInput.SecondaryProperties.First(x => x.Name == eSecondaryProperties.City.ToString()).Value = city?.LongName ?? "";

                                    // State/Province (Administrative Area Level 1)
                                    var state = addressComponents.FirstOrDefault(x => x.Types.Contains(GoogleApi.Entities.Common.Enums.AddressComponentType.Administrative_Area_Level_1));
                                    customInput.SecondaryProperties.First(x => x.Name == eSecondaryProperties.State.ToString()).Value = state?.LongName ?? "";

                                    // Postal Code
                                    var postalCode = addressComponents.FirstOrDefault(x => x.Types.Contains(GoogleApi.Entities.Common.Enums.AddressComponentType.Postal_Code));
                                    customInput.SecondaryProperties.First(x => x.Name == eSecondaryProperties.PostalCode.ToString()).Value = postalCode?.LongName ?? "";

                                    // Country
                                    var country = addressComponents.FirstOrDefault(x => x.Types.Contains(GoogleApi.Entities.Common.Enums.AddressComponentType.Country));
                                    customInput.SecondaryProperties.First(x => x.Name == eSecondaryProperties.Country.ToString()).Value = country?.LongName ?? "";
                                }

                                customInput.Status = eResultStatus.Success;
                                customInput.ErrorMessage = "";
                            }
                            else
                            {
                                customInput.Status = eResultStatus.Danger;
                                customInput.ErrorMessage = "Could not find address.";
                            }
                        }
                        else
                        {
                            customInput.Status = eResultStatus.Danger;
                            customInput.ErrorMessage = response.ErrorMessage;
                        }
                       
                    }
                    else
                    {
                        customInput.Status = eResultStatus.Danger;
                        customInput.ErrorMessage = "Please enter an address.";
                    }
                }

                return customInput;
            }
            catch (Exception ex)
            {
                customInput.Status = eResultStatus.Danger;
                customInput.ErrorMessage = $"Error processing address input: {ex.Message}";
                foreach (var sec in customInput.SecondaryProperties)
                {
                    sec.IsVisible = false;
                }
                return customInput;
            }
        }

        public async Task<CustomInputTransfer> ExecuteAsync(CustomInputTransfer customInput, int moduleId, int userId, int siteId, int QuestionId, IDictionary<string, string> context)
        {
            context.Add($"{customInput.Key}.testContext", "TestValue");
            customInput.Status = eResultStatus.Success;
            customInput.ErrorMessage = "";

            return customInput;
        }
    }
}

