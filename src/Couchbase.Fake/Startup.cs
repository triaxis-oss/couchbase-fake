using Couchbase.Fake.Interfaces;
using Couchbase.Fake.Services;

namespace Couchbase.Fake
{
    class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions<FakeCouchbaseOptions>().BindConfiguration("FakeCouchbase");
            services.AddOptions<CouchbaseProtocolOptions>().BindConfiguration("FakeCouchbase");

            services.AddTransient<ICouchbaseProtocol, CouchbaseProtocol>();
            services.AddTransient<ISaslProvider, SaslProvider>();

            services.AddSingleton<IStorageProvider, InMemoryStorage>();
        }

        public void Configure(IApplicationBuilder app)
        {
        }
    }
}