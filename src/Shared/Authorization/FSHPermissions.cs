using System.Collections.ObjectModel;

namespace FSH.WebApi.Shared.Authorization;

public static class FSHAction
{
    public const string View = nameof(View);
    public const string Search = nameof(Search);
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string Export = nameof(Export);
    public const string Generate = nameof(Generate);
    public const string Clean = nameof(Clean);
    public const string UpgradeSubscription = nameof(UpgradeSubscription);
    
 
}

public static class FSHResource
{
    public const string Tenants = nameof(Tenants);
    public const string Dashboard = nameof(Dashboard);
    public const string Hangfire = nameof(Hangfire);
    public const string Users = nameof(Users);
    public const string UserRoles = nameof(UserRoles);
    public const string Roles = nameof(Roles);
    public const string RoleClaims = nameof(RoleClaims);
    public const string Products = nameof(Products);
    public const string Brands = nameof(Brands);
    public const string RuralGovs=nameof(RuralGovs);
    public const string Natives=nameof(Natives);
    public const string Years = nameof(Years);
    public const string Contributions = nameof(Contributions);
    public const string Reports = nameof(Reports);
    public const string HomePage = nameof(HomePage);
    public const string FinSupport = nameof(FinSupport);
    public const string FSContributions = nameof(FSContributions);
    public const string Logs=nameof(Logs);
         

}

public static class FSHPermissions
{
    private static readonly FSHPermission[] _all = new FSHPermission[]
    {
        new ("View Logs",FSHAction.View,FSHResource.Logs),
        new("View Dashboard", FSHAction.View, FSHResource.Dashboard),
        new("View Hangfire", FSHAction.View, FSHResource.Hangfire),
        new("View Users", FSHAction.View, FSHResource.Users),
        new("Search Users", FSHAction.Search, FSHResource.Users),
        new("Create Users", FSHAction.Create, FSHResource.Users),
        new("Update Users", FSHAction.Update, FSHResource.Users),
        new("Delete Users", FSHAction.Delete, FSHResource.Users),
        new("Export Users", FSHAction.Export, FSHResource.Users),
        new("View UserRoles", FSHAction.View, FSHResource.UserRoles),
        new("Update UserRoles", FSHAction.Update, FSHResource.UserRoles),
        new("View Roles", FSHAction.View, FSHResource.Roles),
        new("Create Roles", FSHAction.Create, FSHResource.Roles),
        new("Update Roles", FSHAction.Update, FSHResource.Roles),
        new("Delete Roles", FSHAction.Delete, FSHResource.Roles),
        new("View RoleClaims", FSHAction.View, FSHResource.RoleClaims),
        new("Update RoleClaims", FSHAction.Update, FSHResource.RoleClaims),

        new("View Products", FSHAction.View, FSHResource.Products, IsBasic: true),
        new("Search Products", FSHAction.Search, FSHResource.Products, IsBasic: true),
        new("Create Products", FSHAction.Create, FSHResource.Products),
        new("Update Products", FSHAction.Update, FSHResource.Products),
        new("Delete Products", FSHAction.Delete, FSHResource.Products),
        new("Export Products", FSHAction.Export, FSHResource.Products),

        new("View Brands", FSHAction.View, FSHResource.Brands, IsBasic: true),
        new("Search Brands", FSHAction.Search, FSHResource.Brands, IsBasic: true),
        new("Create Brands", FSHAction.Create, FSHResource.Brands),
        new("Update Brands", FSHAction.Update, FSHResource.Brands),
        new("Delete Brands", FSHAction.Delete, FSHResource.Brands),
        new("Generate Brands", FSHAction.Generate, FSHResource.Brands),
        new("Clean Brands", FSHAction.Clean, FSHResource.Brands),

        new("View RuralGovs", FSHAction.View, FSHResource.RuralGovs, IsBasic: true),
        new("Search RuralGovs", FSHAction.Search, FSHResource.RuralGovs, IsBasic: true),
        new("Create RuralGovs", FSHAction.Create, FSHResource.RuralGovs),
        new("Update RuralGovs", FSHAction.Update, FSHResource.RuralGovs),
        new("Delete RuralGovs", FSHAction.Delete, FSHResource.RuralGovs),
        new("Clean RuralGovs", FSHAction.Clean, FSHResource.RuralGovs),

         new("View Natives", FSHAction.View, FSHResource.Natives, IsBasic: true),
        new("Search Natives", FSHAction.Search, FSHResource.Natives, IsBasic: true),
        new("Create Natives", FSHAction.Create, FSHResource.Natives),
        new("Update Natives", FSHAction.Update, FSHResource.Natives),
        new("Delete Natives", FSHAction.Delete, FSHResource.Natives),
        new("Export Natives", FSHAction.Export, FSHResource.Natives),

          new("View FinSupport", FSHAction.View, FSHResource.FinSupport, IsBasic: true),
        new("Search FinSupport", FSHAction.Search, FSHResource.FinSupport, IsBasic: true),
        new("Create FinSupport", FSHAction.Create, FSHResource.FinSupport),
        new("Update FinSupport", FSHAction.Update, FSHResource.FinSupport),
        new("Delete FinSupport", FSHAction.Delete, FSHResource.FinSupport),
        new("Export FinSupport", FSHAction.Export, FSHResource.FinSupport),


        new("View Years", FSHAction.View, FSHResource.Years, IsBasic: true),
        new("Search Years", FSHAction.Search, FSHResource.Years, IsBasic: true),
        new("Create Years", FSHAction.Create, FSHResource.Years),
        new("Update Years", FSHAction.Update, FSHResource.Years),
        new("Delete Years", FSHAction.Delete, FSHResource.Years),
        new("Export Years", FSHAction.Export, FSHResource.Years),

        new("View Contributions", FSHAction.View, FSHResource.Contributions, IsBasic: true),
        new("Search Contributions", FSHAction.Search, FSHResource.Contributions, IsBasic: true),
        new("Create Contributions", FSHAction.Create, FSHResource.Contributions),
        new("Update Contributions", FSHAction.Update, FSHResource.Contributions),
        new("Delete Contributions", FSHAction.Delete, FSHResource.Contributions),
        new("Export Contributions", FSHAction.Export, FSHResource.Contributions),

        new("View FSContributions", FSHAction.View, FSHResource.FSContributions, IsBasic: true),
        new("Search FSContributions", FSHAction.Search, FSHResource.FSContributions, IsBasic: true),
        new("Create FSContributions", FSHAction.Create, FSHResource.FSContributions),
        new("Update FSContributions", FSHAction.Update, FSHResource.FSContributions),
        new("Delete FSContributions", FSHAction.Delete, FSHResource.FSContributions),
        new("Export FSContributions", FSHAction.Export, FSHResource.FSContributions),

        new("View Reports", FSHAction.View, FSHResource.Reports, IsBasic: true),
        new("Search Reports", FSHAction.Search, FSHResource.Reports, IsBasic: true),
        new("View Reports", FSHAction.Export, FSHResource.Reports),

        new("View HomePage", FSHAction.View, FSHResource.HomePage, IsBasic: true),

        new("Edit HomePage", FSHAction.Update, FSHResource.HomePage),

        new("View Tenants", FSHAction.View, FSHResource.Tenants, IsRoot: true),
        new("Create Tenants", FSHAction.Create, FSHResource.Tenants, IsRoot: true),
        new("Update Tenants", FSHAction.Update, FSHResource.Tenants, IsRoot: true),
        new("Upgrade Tenant Subscription", FSHAction.UpgradeSubscription, FSHResource.Tenants, IsRoot: true)
    };

    public static IReadOnlyList<FSHPermission> All { get; } = new ReadOnlyCollection<FSHPermission>(_all);
    public static IReadOnlyList<FSHPermission> Root { get; } = new ReadOnlyCollection<FSHPermission>(_all.Where(p => p.IsRoot).ToArray());
    public static IReadOnlyList<FSHPermission> Admin { get; } = new ReadOnlyCollection<FSHPermission>(_all.Where(p => !p.IsRoot).ToArray());
    public static IReadOnlyList<FSHPermission> Basic { get; } = new ReadOnlyCollection<FSHPermission>(_all.Where(p => p.IsBasic).ToArray());
}

public record FSHPermission(string Description, string Action, string Resource, bool IsBasic = false, bool IsRoot = false)
{
    public string Name => NameFor(Action, Resource);
    public static string NameFor(string action, string resource) => $"Permissions.{resource}.{action}";
}
