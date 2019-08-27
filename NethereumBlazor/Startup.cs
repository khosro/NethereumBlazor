using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NethereumBlazor.Services;
using System.Net.Http;
using NethereumBlazor.ViewModels;
using Blazor.FlexGrid;
using Microsoft.Extensions.Http;

namespace NethereumBlazor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            var web3ServiceProvider = new Web3ProviderService();
            var accountsService = new AccountsService(web3ServiceProvider);
            var newBlockProcessingService = new NewBlockProcessingService(web3ServiceProvider);
            var toastsViewModel = new ToastsViewModel();
            var blocksViewModel = new BlocksViewModel(newBlockProcessingService);
            var latestBlockTransactionsViewModel = new LatestBlockTransactionsViewModel(web3ServiceProvider);
            var newAccountPrivateKeyViewModel = new NewAccountPrivateKeyViewModel();
            var accountsViewModel = new AccountsViewModel(accountsService, newAccountPrivateKeyViewModel);

            var accountsTransactionMonitoringService = new AccountsTransactionMonitoringService(accountsService, web3ServiceProvider);




            services.AddSingleton<IWeb3ProviderService, Web3ProviderService>((x) => web3ServiceProvider);
            services.AddSingleton<IAccountsService, AccountsService>((x) => accountsService);
            services.AddSingleton<NewBlockProcessingService>(newBlockProcessingService);
            services.AddSingleton<ToastsViewModel>(toastsViewModel);
            services.AddSingleton<BlocksViewModel>(blocksViewModel);
            services.AddSingleton<LatestBlockTransactionsViewModel>(latestBlockTransactionsViewModel);
            services.AddTransient<BlockTransactionsViewModel>();
            services.AddSingleton<AccountsViewModel>(accountsViewModel);
            services.AddSingleton<NewAccountPrivateKeyViewModel>(newAccountPrivateKeyViewModel);
            services.AddSingleton<SendTransactionViewModel>();
            services.AddSingleton<SendErc20TransactionViewModel>();
            services.AddSingleton(accountsTransactionMonitoringService);
            services.AddSingleton<TransactionWithReceiptViewModel>();



            /*  var type = typeof(ITypedHttpClientFactory<>).Assembly.DefinedTypes.Single(t => t.Name.Contains("DefaultTypedHttpClientFactory"));
              services.AddScoped(typeof(ITypedHttpClientFactory<>), type);
              */
            services.AddFlexGrid(cfg =>
            {
                cfg.ApplyConfiguration(new TransactionsViewModelGridConfiguration());
            }, config => { config.IsServerSideBlazorApp = true; });

            services.AddSingleton<Web3UrlViewModel>();


            if (!services.Any(x => x.ServiceType == typeof(HttpClient)))
            {
                // Setup HttpClient for server side in a client side compatible fashion
                services.AddScoped<HttpClient>(s =>
                {
                    // Creating the URI helper needs to wait until the JS Runtime is initialized, so defer it.
                    var uriHelper = s.GetRequiredService<IUriHelper>();
                    return new HttpClient
                    {
                        BaseAddress = new Uri(uriHelper.GetBaseUri())
                    };
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub<App>(selector: "app");
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
