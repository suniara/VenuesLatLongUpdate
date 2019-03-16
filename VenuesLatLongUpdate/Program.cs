using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Dfc.CourseDirectory.Models.Models.Venues;
using Dfc.CourseDirectory.Services.Interfaces.VenueService;
using Dfc.CourseDirectory.Services.VenueService;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VenuesLatLongUpdate
{
    public class Program
    {
        public static IConfiguration Configuration { get; set; }
        public static IConfigurationRoot Configuration2;

        static void Main(string[] args)
        {
            // get all venues
            var serviceProvider = ConfigureServices();
            var venueService = serviceProvider.GetService<IVenueService>();
            var venues = venueService.GetAllVenues().Result;


            Configuration2 = SetUpConfiguration();
            ISearchIndexClient indexClientForQueries = CreateSearchIndexClient(Configuration2);

            int counter = 0;
            int actualVenuesUpdated = 0;
            int numberOfVenues = venues.Value.Count;

            foreach (var venue in venues.Value)
            {
                counter++;
                var onspdPostcode = RunQueries(indexClientForQueries, venue.PostCode);

                if (onspdPostcode != null)
                {
                    
                    venue.Latitude = onspdPostcode.lat;
                    venue.Longitude = onspdPostcode.@long;
                    var updatedVenue = venueService.UpdateAsync(venue).Result.Value;
                    actualVenuesUpdated++;
                    //GetVenueByIdCriteria criteria = new GetVenueByIdCriteria(venue.ID);
                    //var addedVenue = venueService.GetVenueByIdAsync(criteria).Result.Value;
                }
                Console.WriteLine($"{counter} of {numberOfVenues} completed.");
                Console.WriteLine($"{actualVenuesUpdated} of {numberOfVenues} actually completed.");
            }

            Console.WriteLine("Finished");
            Console.ReadLine();
        }

        private static IConfigurationRoot SetUpConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            return builder.Build();
        }


        public static  ServiceProvider ConfigureServices()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional:true, reloadOnChange: true);

            Configuration = builder.Build();

            var services = new ServiceCollection();
            services.AddSingleton(Configuration);
            services.Configure<VenueServiceSettings>(Configuration.GetSection("VenueServiceSettings"));
            services.AddScoped<IVenueService , VenueService>();

            services.AddTransient(provider => new HttpClient());
            services.AddLogging();

            return services.BuildServiceProvider();
        }


        private static SearchIndexClient CreateSearchIndexClient(IConfigurationRoot configuration)
        {
            string searchServiceName = configuration["SearchServiceName"];
            string queryApiKey = configuration["SearchServiceQueryApiKey"];

            SearchIndexClient indexClient = new SearchIndexClient(searchServiceName, "onspd", new SearchCredentials(queryApiKey));
            return indexClient;
        }

        private static ONSPD RunQueries(ISearchIndexClient indexClient, string venuePostcode)
        {
            SearchParameters parameters;
            DocumentSearchResult<ONSPD> results;

            parameters = new SearchParameters
            {
                Select = new[] {"pcd", "pcd2", "pcds", "Parish", "LocalAuthority", "Region" , "County", "Country", "lat", "long"},
                SearchMode = SearchMode.All,
                Top = 1,
                QueryType = QueryType.Full
            };

            results = indexClient.Documents.Search<ONSPD>(venuePostcode, parameters);

            WriteDocuments(results);

            if (results.Results != null && results.Results.Any())
            {
                return results.Results.First().Document;
            }

            return null;
        }

        private static void WriteDocuments(DocumentSearchResult<ONSPD> searchResults)
        {
            foreach (SearchResult<ONSPD> result in searchResults.Results)
            {
                Console.WriteLine(result.Document);
            }

            Console.WriteLine();
        }
    }
}
