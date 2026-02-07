using MicroCommerce.ApiService.Features.Catalog.Domain.Entities;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MicroCommerce.ApiService.Features.Catalog.Infrastructure;

/// <summary>
/// Seeds the catalog database with sample electronics products.
/// Only runs in Development environment and only when the database is empty.
/// </summary>
public sealed class CatalogDataSeeder : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<CatalogDataSeeder> _logger;

    public CatalogDataSeeder(
        IServiceScopeFactory scopeFactory,
        IHostEnvironment environment,
        ILogger<CatalogDataSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _environment = environment;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_environment.IsDevelopment())
        {
            _logger.LogInformation("Skipping catalog data seeding - not in Development environment");
            return;
        }

        // Small delay to ensure migrations have been applied
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        try
        {
            // Idempotency guard: only seed when categories table is empty
            if (await context.Categories.AnyAsync(stoppingToken))
            {
                _logger.LogInformation("Catalog data already exists, skipping seeding");
                return;
            }

            _logger.LogInformation("Seeding catalog with sample electronics products...");

            var categories = CreateCategories();
            context.Categories.AddRange(categories);

            var products = CreateProducts(categories);
            context.Products.AddRange(products);

            await context.SaveChangesAsync(stoppingToken);

            _logger.LogInformation(
                "Catalog seeding complete: {CategoryCount} categories, {ProductCount} products",
                categories.Count,
                products.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding catalog data");
        }
    }

    private static List<Category> CreateCategories()
    {
        return
        [
            Category.Create(CategoryName.Create("Laptops"), "Portable computers for work and play"),
            Category.Create(CategoryName.Create("Smartphones"), "Mobile phones with advanced features"),
            Category.Create(CategoryName.Create("Tablets"), "Touchscreen portable computing devices"),
            Category.Create(CategoryName.Create("Audio"), "Headphones, speakers, and audio equipment"),
            Category.Create(CategoryName.Create("Accessories"), "Cables, cases, and peripherals"),
            Category.Create(CategoryName.Create("Monitors"), "Desktop displays and screens"),
            Category.Create(CategoryName.Create("Gaming"), "Gaming consoles, controllers, and gear"),
            Category.Create(CategoryName.Create("Wearables"), "Smartwatches and fitness trackers"),
        ];
    }

    private static List<Product> CreateProducts(List<Category> categories)
    {
        var laptops = categories[0];
        var smartphones = categories[1];
        var tablets = categories[2];
        var audio = categories[3];
        var accessories = categories[4];
        var monitors = categories[5];
        var gaming = categories[6];
        var wearables = categories[7];

        var products = new List<Product>();

        // Laptops (8 products)
        AddProduct(products, "ProBook 15 Laptop", "15.6-inch Full HD display, Intel Core i7, 16GB RAM, 512GB SSD", 1299.99m, laptops, "LAP-001");
        AddProduct(products, "UltraSlim 14 Notebook", "14-inch 2K display, AMD Ryzen 7, 16GB RAM, 1TB SSD", 1099.99m, laptops, "LAP-002");
        AddProduct(products, "PowerStation Pro 17", "17.3-inch 4K display, Intel Core i9, 32GB RAM, 1TB SSD, RTX 4070", 2299.99m, laptops, "LAP-003");
        AddProduct(products, "StudentBook Air 13", "13.3-inch display, Intel Core i5, 8GB RAM, 256GB SSD, lightweight", 649.99m, laptops, "LAP-004");
        AddProduct(products, "ChromeBook Essential", "14-inch HD display, MediaTek processor, 4GB RAM, 64GB eMMC", 249.99m, laptops, "LAP-005");
        AddProduct(products, "WorkStation Elite 16", "16-inch Retina display, Apple M3 Pro, 18GB RAM, 512GB SSD", 2499.99m, laptops, "LAP-006");
        AddProduct(products, "GamerForce 15X", "15.6-inch 165Hz display, Intel Core i7, 16GB RAM, RTX 4060", 1599.99m, laptops, "LAP-007");
        AddProduct(products, "TravelMate Compact 12", "12.4-inch touch display, Intel Core i5, 8GB RAM, 256GB SSD, 2-in-1", 899.99m, laptops, "LAP-008");

        // Smartphones (8 products)
        AddProduct(products, "Galaxy Ultra X", "6.8-inch Dynamic AMOLED, 200MP camera, 256GB, 5G", 1199.99m, smartphones, "PHN-001");
        AddProduct(products, "Pixel Pro 9", "6.7-inch LTPO OLED, 50MP camera, Tensor G4, 128GB", 999.99m, smartphones, "PHN-002");
        AddProduct(products, "iPhone Nova 16", "6.1-inch Super Retina XDR, A19 chip, 128GB", 1099.99m, smartphones, "PHN-003");
        AddProduct(products, "OnePlus Turbo 13", "6.7-inch AMOLED 120Hz, Snapdragon 8 Gen 4, 256GB", 749.99m, smartphones, "PHN-004");
        AddProduct(products, "Budget Phone SE", "6.5-inch LCD, quad camera, 64GB, 5000mAh battery", 199.99m, smartphones, "PHN-005");
        AddProduct(products, "FoldMax Pro", "7.6-inch foldable display, Snapdragon 8 Gen 4, 512GB", 1799.99m, smartphones, "PHN-006");
        AddProduct(products, "Compact Mini 6", "5.4-inch OLED, compact design, 128GB, flagship specs", 699.99m, smartphones, "PHN-007");
        AddProduct(products, "Rugged Explorer X1", "6.3-inch Gorilla Glass, IP69 rating, 5G, 128GB", 549.99m, smartphones, "PHN-008");

        // Tablets (6 products)
        AddProduct(products, "iPad Pro Vision", "12.9-inch Liquid Retina XDR, M3 chip, 256GB", 1099.99m, tablets, "TAB-001");
        AddProduct(products, "Galaxy Tab Ultra", "14.6-inch AMOLED, Snapdragon 8 Gen 3, 256GB, S Pen", 1199.99m, tablets, "TAB-002");
        AddProduct(products, "Surface Go 5", "10.5-inch PixelSense, Intel Core i3, 8GB RAM, 128GB", 449.99m, tablets, "TAB-003");
        AddProduct(products, "Fire HD 10 Plus", "10.1-inch Full HD, octa-core, 64GB, wireless charging", 179.99m, tablets, "TAB-004");
        AddProduct(products, "Drawing Pad Pro 16", "15.6-inch laminated display, 8192 pressure levels, stylus", 599.99m, tablets, "TAB-005");
        AddProduct(products, "Kids Tab Safe 8", "8-inch HD, kid-proof case, parental controls, 32GB", 129.99m, tablets, "TAB-006");

        // Audio (7 products)
        AddProduct(products, "UltraSound Pro Headphones", "Over-ear, active noise cancelling, 40hr battery, Hi-Res audio", 349.99m, audio, "AUD-001");
        AddProduct(products, "BassBoost Wireless Earbuds", "True wireless, ANC, 8hr battery, IPX5 waterproof", 149.99m, audio, "AUD-002");
        AddProduct(products, "Studio Monitor MK3", "Professional reference headphones, flat response, 250 ohm", 249.99m, audio, "AUD-003");
        AddProduct(products, "Portable Boom Speaker", "360-degree sound, 20hr battery, IP67 waterproof, Bluetooth 5.3", 129.99m, audio, "AUD-004");
        AddProduct(products, "SoundBar Cinema 500", "5.1 channel soundbar with wireless subwoofer, Dolby Atmos", 499.99m, audio, "AUD-005");
        AddProduct(products, "Podcast Mic Pro USB", "Condenser USB microphone, cardioid, pop filter included", 89.99m, audio, "AUD-006");
        AddProduct(products, "Vinyl Turntable Classic", "Belt-drive turntable, built-in preamp, Bluetooth output", 199.99m, audio, "AUD-007");

        // Accessories (6 products)
        AddProduct(products, "USB-C Hub 8-in-1", "HDMI 4K, USB-A, SD card, Ethernet, PD 100W charging", 59.99m, accessories, "ACC-001");
        AddProduct(products, "Wireless Charging Pad Duo", "15W fast charging, compatible with phones and earbuds", 39.99m, accessories, "ACC-002");
        AddProduct(products, "Mechanical Keyboard RGB", "Hot-swappable switches, PBT keycaps, wireless/wired", 109.99m, accessories, "ACC-003");
        AddProduct(products, "Ergonomic Mouse Pro", "Vertical design, 4000 DPI, Bluetooth + USB receiver", 49.99m, accessories, "ACC-004");
        AddProduct(products, "Laptop Stand Adjustable", "Aluminum, height adjustable, foldable, heat dissipation", 34.99m, accessories, "ACC-005");
        AddProduct(products, "Cable Kit Essentials", "USB-C, Lightning, HDMI cables bundle, braided nylon", 24.99m, accessories, "ACC-006");

        // Monitors (5 products)
        AddProduct(products, "UltraWide 34 Curved", "34-inch UWQHD, 144Hz, 1ms, USB-C hub, HDR400", 699.99m, monitors, "MON-001");
        AddProduct(products, "4K Creator Display 27", "27-inch 4K UHD, 100% sRGB, factory calibrated, USB-C PD", 549.99m, monitors, "MON-002");
        AddProduct(products, "Gaming Monitor 27 QHD", "27-inch QHD, 240Hz, 1ms, G-Sync compatible, HDR600", 449.99m, monitors, "MON-003");
        AddProduct(products, "Portable Monitor 15.6", "15.6-inch Full HD, USB-C powered, magnetic cover stand", 199.99m, monitors, "MON-004");
        AddProduct(products, "Office Display 24 FHD", "24-inch Full HD, IPS, blue light filter, adjustable stand", 179.99m, monitors, "MON-005");

        // Gaming (5 products)
        AddProduct(products, "ProController Elite V2", "Wireless controller, Hall effect sticks, RGB, macro buttons", 79.99m, gaming, "GAM-001");
        AddProduct(products, "Gaming Headset 7.1", "Virtual surround sound, detachable mic, RGB, USB/3.5mm", 69.99m, gaming, "GAM-002");
        AddProduct(products, "Stream Deck Mini 6-Key", "Customizable LCD keys, macro support, plugin ecosystem", 79.99m, gaming, "GAM-003");
        AddProduct(products, "Gaming Mouse Pad XL", "900x400mm, RGB edge lighting, micro-texture surface", 29.99m, gaming, "GAM-004");
        AddProduct(products, "Capture Card 4K HDR", "4K60 passthrough, 1080p60 capture, USB 3.0, low latency", 149.99m, gaming, "GAM-005");

        // Wearables (5 products)
        AddProduct(products, "SmartWatch Pro Ultra", "1.9-inch AMOLED, GPS, heart rate, SpO2, 14-day battery", 399.99m, wearables, "WRB-001");
        AddProduct(products, "Fitness Band 8", "AMOLED display, 150+ sports modes, SpO2, sleep tracking", 49.99m, wearables, "WRB-002");
        AddProduct(products, "Smart Ring Health", "Titanium, heart rate, sleep, SpO2, temperature tracking", 299.99m, wearables, "WRB-003");
        AddProduct(products, "Kids SmartWatch GPS", "GPS tracking, SOS button, camera, water resistant", 79.99m, wearables, "WRB-004");
        AddProduct(products, "AR Smart Glasses Lite", "Micro-LED display, voice assistant, navigation, 4hr battery", 499.99m, wearables, "WRB-005");

        return products;
    }

    private static void AddProduct(
        List<Product> products,
        string name,
        string description,
        decimal price,
        Category category,
        string sku)
    {
        var encodedName = Uri.EscapeDataString(name);
        var imageUrl = $"https://placehold.co/600x400/EEE/31343C?text={encodedName}";

        var product = Product.Create(
            ProductName.Create(name),
            description,
            Money.Create(price),
            category.Id,
            imageUrl,
            sku);

        product.Publish();

        products.Add(product);
    }
}
