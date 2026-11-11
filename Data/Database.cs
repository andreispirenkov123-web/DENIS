using HouseholdGoodsIS.Models;
using Npgsql;

namespace HouseholdGoodsIS.Data;

/// <summary>Доступ к базе данных PostgreSQL (АИС «Бытовые товары»).</summary>
public static class Database
{
    public const string ConnectionString =
        "Host=127.0.0.1;Port=5432;Database=household_goods_db;Username=postgres;Password=1234;" +
        "Encoding=UTF8;Timeout=5;Command Timeout=10";

    public static NpgsqlConnection CreateConnection()
    {
        var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    public static string TestConnection()
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand("SELECT version()", connection);
        return ((string?)command.ExecuteScalar() ?? string.Empty).Split(',')[0];
    }

    // ======================= ПОЛЬЗОВАТЕЛИ =======================
    public static AppUser? Authorize(string login, string password)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT u.id, u.last_name, u.first_name, u.middle_name, u.email, u.phone, u.login,
                     u.registered_at, u.role_id, r.name AS role_name, u.avatar_path,
                     u.is_blocked, u.last_login
              FROM app_user u JOIN role r ON r.id = u.role_id
              WHERE u.login = @login AND u.password = @password", connection);
        command.Parameters.AddWithValue("login", login);
        command.Parameters.AddWithValue("password", password);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadUser(reader) : null;
    }

    public static void UpdateLastLogin(int userId)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand("UPDATE app_user SET last_login = now() WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", userId);
        command.ExecuteNonQuery();
    }

    public static bool VerifyPassword(int userId, string password)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand("SELECT count(*) FROM app_user WHERE id = @id AND password = @p", connection);
        command.Parameters.AddWithValue("id", userId);
        command.Parameters.AddWithValue("p", password);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public static void ChangePassword(int userId, string newPassword)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand("UPDATE app_user SET password = @p WHERE id = @id", connection);
        command.Parameters.AddWithValue("p", newPassword);
        command.Parameters.AddWithValue("id", userId);
        command.ExecuteNonQuery();
    }

    public static List<AppUser> GetUsers(string search = "")
    {
        var list = new List<AppUser>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT u.id, u.last_name, u.first_name, u.middle_name, u.email, u.phone, u.login,
                     u.registered_at, u.role_id, r.name AS role_name, u.avatar_path, u.is_blocked, u.last_login
              FROM app_user u JOIN role r ON r.id = u.role_id
              WHERE @s = '' OR u.last_name ILIKE '%' || @s || '%' OR u.first_name ILIKE '%' || @s || '%'
                    OR u.login ILIKE '%' || @s || '%' OR r.name ILIKE '%' || @s || '%'
              ORDER BY u.last_name, u.first_name", connection);
        command.Parameters.AddWithValue("s", search);
        using var reader = command.ExecuteReader();
        while (reader.Read()) list.Add(ReadUser(reader));
        return list;
    }

    public static AppUser? GetUser(int id)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT u.id, u.last_name, u.first_name, u.middle_name, u.email, u.phone, u.login,
                     u.registered_at, u.role_id, r.name AS role_name, u.avatar_path, u.is_blocked, u.last_login
              FROM app_user u JOIN role r ON r.id = u.role_id WHERE u.id = @id", connection);
        command.Parameters.AddWithValue("id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadUser(reader) : null;
    }

    public static int SaveUser(AppUser user, string? newPassword)
    {
        using var connection = CreateConnection();
        if (user.Id == 0)
        {
            using var command = new NpgsqlCommand(
                @"INSERT INTO app_user (last_name, first_name, middle_name, email, phone, login, password,
                                        role_id, is_blocked, registered_at)
                  VALUES (@ln, @fn, @mn, @email, @phone, @login, @pwd, @role, @blocked, CURRENT_DATE)
                  RETURNING id", connection);
            AddUserParams(command, user);
            command.Parameters.AddWithValue("pwd", newPassword ?? "12345");
            return Convert.ToInt32(command.ExecuteScalar());
        }
        using (var command = new NpgsqlCommand(
                   @"UPDATE app_user SET last_name=@ln, first_name=@fn, middle_name=@mn, email=@email,
                          phone=@phone, login=@login, role_id=@role, is_blocked=@blocked,
                          password=COALESCE(@pwd, password) WHERE id=@id", connection))
        {
            AddUserParams(command, user);
            command.Parameters.AddWithValue("id", user.Id);
            command.Parameters.AddWithValue("pwd", (object?)newPassword ?? DBNull.Value);
            command.ExecuteNonQuery();
            return user.Id;
        }
    }

    private static void AddUserParams(NpgsqlCommand command, AppUser user)
    {
        command.Parameters.AddWithValue("ln", user.LastName);
        command.Parameters.AddWithValue("fn", user.FirstName);
        command.Parameters.AddWithValue("mn", (object?)user.MiddleName ?? DBNull.Value);
        command.Parameters.AddWithValue("email", user.Email);
        command.Parameters.AddWithValue("phone", user.Phone);
        command.Parameters.AddWithValue("login", user.Login);
        command.Parameters.AddWithValue("role", user.RoleId);
        command.Parameters.AddWithValue("blocked", user.IsBlocked);
    }

    public static void SetBlocked(int userId, bool blocked)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand("UPDATE app_user SET is_blocked = @b WHERE id = @id", connection);
        command.Parameters.AddWithValue("b", blocked);
        command.Parameters.AddWithValue("id", userId);
        command.ExecuteNonQuery();
    }

    public static void DeleteUser(int id)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand("DELETE FROM app_user WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", id);
        command.ExecuteNonQuery();
    }

    public static List<Role> GetRoles()
    {
        var list = new List<Role>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand("SELECT id, name FROM role ORDER BY id", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read()) list.Add(new Role { Id = reader.GetInt32(0), Name = reader.GetString(1) });
        return list;
    }

    private static AppUser ReadUser(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        LastName = reader.GetString(1),
        FirstName = reader.GetString(2),
        MiddleName = reader.IsDBNull(3) ? null : reader.GetString(3),
        Email = reader.GetString(4),
        Phone = reader.GetString(5),
        Login = reader.GetString(6),
        RegisteredAt = reader.GetDateTime(7),
        RoleId = reader.GetInt32(8),
        RoleName = reader.GetString(9),
        AvatarPath = reader.IsDBNull(10) ? null : reader.GetString(10),
        IsBlocked = reader.GetBoolean(11),
        LastLogin = reader.IsDBNull(12) ? null : reader.GetDateTime(12)
    };

    // ======================= ТОВАРЫ =======================
    public static List<Product> GetProducts(string search = "", string category = "", int sortMode = 0)
    {
        var list = new List<Product>();
        var orderBy = sortMode switch
        {
            1 => "p.retail_price ASC",
            2 => "p.retail_price DESC",
            3 => "ps.stock ASC",
            4 => "p.name DESC",
            _ => "p.name"
        };
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            $@"SELECT p.id, p.name, p.category_id, c.name, p.supplier_id, s.name, p.manufacturer,
                      p.model, p.sku, p.purchase_price, p.retail_price, p.warranty_months,
                      p.description, p.full_description, p.image_path, p.old_price, p.rating, p.reviews_count, p.min_stock, p.is_active, COALESCE(ps.stock, 0)
               FROM product p
               JOIN category c ON c.id = p.category_id
               JOIN supplier s ON s.id = p.supplier_id
               LEFT JOIN product_stock ps ON ps.product_id = p.id
               WHERE (@s = '' OR p.name ILIKE '%' || @s || '%' OR p.sku ILIKE '%' || @s || '%'
                      OR p.manufacturer ILIKE '%' || @s || '%' OR p.model ILIKE '%' || @s || '%')
                 AND (@cat = '' OR c.name = @cat)
               ORDER BY {orderBy}", connection);
        command.Parameters.AddWithValue("s", search);
        command.Parameters.AddWithValue("cat", category);
        using var reader = command.ExecuteReader();
        while (reader.Read()) list.Add(ReadProduct(reader));
        return list;
    }

    public static Product? GetProduct(int id)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT p.id, p.name, p.category_id, c.name, p.supplier_id, s.name, p.manufacturer,
                     p.model, p.sku, p.purchase_price, p.retail_price, p.warranty_months,
                     p.description, p.full_description, p.image_path, p.old_price, p.rating, p.reviews_count, p.min_stock, p.is_active, COALESCE(ps.stock, 0)
              FROM product p
              JOIN category c ON c.id = p.category_id
              JOIN supplier s ON s.id = p.supplier_id
              LEFT JOIN product_stock ps ON ps.product_id = p.id
              WHERE p.id = @id", connection);
        command.Parameters.AddWithValue("id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadProduct(reader) : null;
    }

    private static Product ReadProduct(NpgsqlDataReader r) => new()
    {
        Id = r.GetInt32(0), Name = r.GetString(1), CategoryId = r.GetInt32(2), CategoryName = r.GetString(3),
        SupplierId = r.GetInt32(4), SupplierName = r.GetString(5), Manufacturer = r.GetString(6),
        Model = r.GetString(7), Sku = r.GetString(8), PurchasePrice = r.GetDecimal(9),
        RetailPrice = r.GetDecimal(10), WarrantyMonths = r.GetInt32(11),
        Description = r.IsDBNull(12) ? null : r.GetString(12),
        FullDescription = r.IsDBNull(13) ? null : r.GetString(13),
        ImagePath = r.IsDBNull(14) ? null : r.GetString(14),
        OldPrice = r.IsDBNull(15) ? null : r.GetDecimal(15),
        Rating = r.GetDecimal(16), ReviewsCount = r.GetInt32(17),
        MinStock = r.GetInt32(18), IsActive = r.GetBoolean(19), Stock = r.GetInt32(20)
    };

    public static int SaveProduct(Product p)
    {
        using var connection = CreateConnection();
        if (p.Id == 0)
        {
            using var command = new NpgsqlCommand(
                @"INSERT INTO product (name, category_id, supplier_id, manufacturer, model, sku,
                                       purchase_price, retail_price, old_price, warranty_months,
                                       description, image_path, min_stock, is_active)
                  VALUES (@name,@cat,@sup,@man,@model,@sku,@purchase,@retail,@oldprice,@warranty,
                          @desc,@image,@minstock,@active)
                  RETURNING id", connection);
            AddProductParams(command, p);
            return Convert.ToInt32(command.ExecuteScalar());
        }
        using (var command = new NpgsqlCommand(
                   @"UPDATE product SET name=@name, category_id=@cat, supplier_id=@sup, manufacturer=@man,
                          model=@model, sku=@sku, purchase_price=@purchase, retail_price=@retail,
                          old_price=@oldprice, warranty_months=@warranty, description=@desc,
                          image_path=@image, min_stock=@minstock, is_active=@active
                     WHERE id=@id", connection))
        {
            AddProductParams(command, p);
            command.Parameters.AddWithValue("id", p.Id);
            command.ExecuteNonQuery();
            return p.Id;
        }
    }

    private static void AddProductParams(NpgsqlCommand command, Product p)
    {
        command.Parameters.AddWithValue("name", p.Name);
        command.Parameters.AddWithValue("cat", p.CategoryId);
        command.Parameters.AddWithValue("sup", p.SupplierId);
        command.Parameters.AddWithValue("man", p.Manufacturer);
        command.Parameters.AddWithValue("model", p.Model);
        command.Parameters.AddWithValue("sku", p.Sku);
        command.Parameters.AddWithValue("purchase", p.PurchasePrice);
        command.Parameters.AddWithValue("retail", p.RetailPrice);
        command.Parameters.AddWithValue("oldprice", (object?)p.OldPrice ?? DBNull.Value);
        command.Parameters.AddWithValue("minstock", p.MinStock);
        command.Parameters.AddWithValue("warranty", p.WarrantyMonths);
        command.Parameters.AddWithValue("desc", (object?)p.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("image", (object?)p.ImagePath ?? DBNull.Value);
        command.Parameters.AddWithValue("active", p.IsActive);
    }

    public static void DeleteProduct(int id)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand("DELETE FROM product WHERE id=@id", connection);
        command.Parameters.AddWithValue("id", id);
        command.ExecuteNonQuery();
    }

    public static StockSummary GetStockSummary()
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT count(*), COALESCE(SUM(ps.stock),0),
                     COALESCE(SUM(ps.stock * p.purchase_price),0),
                     COALESCE(SUM(ps.stock * p.retail_price),0),
                     COALESCE(SUM(CASE WHEN ps.stock <= p.min_stock THEN 1 ELSE 0 END),0)
              FROM product_stock ps JOIN product p ON p.id = ps.product_id", connection);
        using var reader = command.ExecuteReader();
        reader.Read();
        return new StockSummary
        {
            ProductsCount = reader.GetInt32(0),
            UnitsCount = Convert.ToInt32(reader.GetDecimal(1)),
            PurchaseValue = reader.GetDecimal(2),
            RetailValue = reader.GetDecimal(3),
            LowStockCount = Convert.ToInt32(reader.GetInt64(4))
        };
    }

    public static List<StockMovement> GetStockMovements(int productId = 0, int limit = 120)
    {
        var list = new List<StockMovement>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT movement_type, document_id, document, product_id, product_name, sku,
                     quantity, movement_date, user_name, comment
              FROM stock_movements
              WHERE (@pid = 0 OR product_id = @pid)
              ORDER BY movement_date DESC, document_id DESC
              LIMIT @limit", connection);
        command.Parameters.AddWithValue("pid", productId);
        command.Parameters.AddWithValue("limit", limit);
        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(new StockMovement
            {
                MovementType = reader.GetString(0), DocumentId = reader.GetInt32(1),
                Document = reader.GetString(2), ProductId = reader.GetInt32(3),
                ProductName = reader.GetString(4), Sku = reader.GetString(5),
                Quantity = reader.GetInt32(6), MovementDate = reader.GetDateTime(7),
                UserName = reader.GetString(8), Comment = reader.IsDBNull(9) ? null : reader.GetString(9)
            });
        return list;
    }

    public static int SaveStockAdjustment(int productId, int delta, string reason, string? comment, int userId)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"INSERT INTO stock_adjustment (product_id, quantity_delta, reason, comment, user_id)
              VALUES (@product, @delta, @reason, @comment, @user) RETURNING id", connection);
        command.Parameters.AddWithValue("product", productId);
        command.Parameters.AddWithValue("delta", delta);
        command.Parameters.AddWithValue("reason", reason);
        command.Parameters.AddWithValue("comment", (object?)comment ?? DBNull.Value);
        command.Parameters.AddWithValue("user", userId);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public static List<ProductAttribute> GetProductAttributes(int productId)
    {
        var list = new List<ProductAttribute>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT id, product_id, group_name, name, value, sort_order
              FROM product_attribute WHERE product_id = @id
              ORDER BY sort_order, id", connection);
        command.Parameters.AddWithValue("id", productId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(new ProductAttribute
            {
                Id = reader.GetInt32(0), ProductId = reader.GetInt32(1),
                GroupName = reader.GetString(2), Name = reader.GetString(3),
                Value = reader.GetString(4), SortOrder = reader.GetInt32(5)
            });
        return list;
    }

    public static List<Category> GetCategoriesWithCounts()
    {
        var list = new List<Category>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT c.id, c.name, c.description, count(p.id)
              FROM category c LEFT JOIN product p ON p.category_id = c.id
              GROUP BY c.id, c.name, c.description ORDER BY c.name", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(new Category
            {
                Id = reader.GetInt32(0), Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                ProductCount = Convert.ToInt32(reader.GetInt64(3))
            });
        return list;
    }

    public static List<Product> GetPopularProducts(int count = 8)
    {
        var list = new List<Product>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT p.id, p.name, p.category_id, c.name, p.supplier_id, s.name, p.manufacturer,
                     p.model, p.sku, p.purchase_price, p.retail_price, p.warranty_months,
                     p.description, p.full_description, p.image_path, p.old_price, p.rating, p.reviews_count,
                     p.min_stock, p.is_active, COALESCE(ps.stock, 0)
              FROM product p
              JOIN category c ON c.id = p.category_id
              JOIN supplier s ON s.id = p.supplier_id
              LEFT JOIN product_stock ps ON ps.product_id = p.id
              WHERE p.is_active = true
              ORDER BY p.rating DESC, p.reviews_count DESC LIMIT @count", connection);
        command.Parameters.AddWithValue("count", count);
        using var reader = command.ExecuteReader();
        while (reader.Read()) list.Add(ReadProduct(reader));
        return list;
    }

    public static int GetStock(int productId)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand("SELECT stock FROM product_stock WHERE product_id=@id", connection);
        command.Parameters.AddWithValue("id", productId);
        var value = command.ExecuteScalar();
        return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
    }

    // ======================= КАТЕГОРИИ =======================
    public static List<Category> GetCategories()
    {
        var list = new List<Category>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand("SELECT id, name, description FROM category ORDER BY name", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(new Category { Id = reader.GetInt32(0), Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2) });
        return list;
    }

    public static int SaveCategory(Category c)
    {
        using var connection = CreateConnection();
        if (c.Id == 0)
        {
            using var command = new NpgsqlCommand(
                "INSERT INTO category (name, description) VALUES (@n,@d) RETURNING id", connection);
            command.Parameters.AddWithValue("n", c.Name);
            command.Parameters.AddWithValue("d", (object?)c.Description ?? DBNull.Value);
            return Convert.ToInt32(command.ExecuteScalar());
        }
        using (var command = new NpgsqlCommand(
                   "UPDATE category SET name=@n, description=@d WHERE id=@id", connection))
        {
            command.Parameters.AddWithValue("n", c.Name);
            command.Parameters.AddWithValue("d", (object?)c.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("id", c.Id);
            command.ExecuteNonQuery();
            return c.Id;
        }
    }

    public static void DeleteCategory(int id)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand("DELETE FROM category WHERE id=@id", connection);
        command.Parameters.AddWithValue("id", id);
        command.ExecuteNonQuery();
    }

    // ======================= ПОСТАВЩИКИ =======================
    public static List<Supplier> GetSuppliers()
    {
        var list = new List<Supplier>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            "SELECT id, name, inn, contact_person, phone, email, city FROM supplier ORDER BY name", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read()) list.Add(ReadSupplier(reader));
        return list;
    }

    public static Supplier? GetSupplier(int id)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            "SELECT id, name, inn, contact_person, phone, email, city FROM supplier WHERE id=@id", connection);
        command.Parameters.AddWithValue("id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadSupplier(reader) : null;
    }

    private static Supplier ReadSupplier(NpgsqlDataReader r) => new()
    {
        Id = r.GetInt32(0), Name = r.GetString(1), Inn = r.IsDBNull(2) ? null : r.GetString(2),
        ContactPerson = r.IsDBNull(3) ? null : r.GetString(3),
        Phone = r.IsDBNull(4) ? null : r.GetString(4),
        Email = r.IsDBNull(5) ? null : r.GetString(5),
        City = r.IsDBNull(6) ? null : r.GetString(6)
    };

    public static int SaveSupplier(Supplier s)
    {
        using var connection = CreateConnection();
        if (s.Id == 0)
        {
            using var command = new NpgsqlCommand(
                @"INSERT INTO supplier (name, inn, contact_person, phone, email, city)
                  VALUES (@n,@inn,@p,@ph,@e,@c) RETURNING id", connection);
            AddSupplierParams(command, s);
            return Convert.ToInt32(command.ExecuteScalar());
        }
        using (var command = new NpgsqlCommand(
                   @"UPDATE supplier SET name=@n, inn=@inn, contact_person=@p, phone=@ph, email=@e,
                          city=@c WHERE id=@id", connection))
        {
            AddSupplierParams(command, s);
            command.Parameters.AddWithValue("id", s.Id);
            command.ExecuteNonQuery();
            return s.Id;
        }
    }

    private static void AddSupplierParams(NpgsqlCommand command, Supplier s)
    {
        command.Parameters.AddWithValue("n", s.Name);
        command.Parameters.AddWithValue("inn", (object?)s.Inn ?? DBNull.Value);
        command.Parameters.AddWithValue("p", (object?)s.ContactPerson ?? DBNull.Value);
        command.Parameters.AddWithValue("ph", (object?)s.Phone ?? DBNull.Value);
        command.Parameters.AddWithValue("e", (object?)s.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("c", (object?)s.City ?? DBNull.Value);
    }

    public static void DeleteSupplier(int id)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand("DELETE FROM supplier WHERE id=@id", connection);
        command.Parameters.AddWithValue("id", id);
        command.ExecuteNonQuery();
    }

    // ======================= КЛИЕНТЫ =======================
    public static List<Client> GetClients(string search = "")
    {
        var list = new List<Client>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT id, last_name, first_name, middle_name, phone, email, city, address, discount_card
              FROM client
              WHERE @s = '' OR last_name ILIKE '%' || @s || '%' OR first_name ILIKE '%' || @s || '%'
                    OR phone ILIKE '%' || @s || '%' OR city ILIKE '%' || @s || '%'
              ORDER BY last_name, first_name", connection);
        command.Parameters.AddWithValue("s", search);
        using var reader = command.ExecuteReader();
        while (reader.Read()) list.Add(ReadClient(reader));
        return list;
    }

    public static Client? GetClient(int id)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT id, last_name, first_name, middle_name, phone, email, city, address, discount_card
              FROM client WHERE id=@id", connection);
        command.Parameters.AddWithValue("id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadClient(reader) : null;
    }

    private static Client ReadClient(NpgsqlDataReader r) => new()
    {
        Id = r.GetInt32(0), LastName = r.GetString(1), FirstName = r.GetString(2),
        MiddleName = r.IsDBNull(3) ? null : r.GetString(3), Phone = r.GetString(4),
        Email = r.IsDBNull(5) ? null : r.GetString(5), City = r.IsDBNull(6) ? null : r.GetString(6),
        Address = r.IsDBNull(7) ? null : r.GetString(7),
        DiscountCard = r.IsDBNull(8) ? null : r.GetString(8)
    };

    public static int SaveClient(Client c)
    {
        using var connection = CreateConnection();
        if (c.Id == 0)
        {
            using var command = new NpgsqlCommand(
                @"INSERT INTO client (last_name, first_name, middle_name, phone, email, city, address, discount_card)
                  VALUES (@ln,@fn,@mn,@phone,@email,@city,@addr,@card) RETURNING id", connection);
            AddClientParams(command, c);
            return Convert.ToInt32(command.ExecuteScalar());
        }
        using (var command = new NpgsqlCommand(
                   @"UPDATE client SET last_name=@ln, first_name=@fn, middle_name=@mn, phone=@phone,
                          email=@email, city=@city, address=@addr, discount_card=@card WHERE id=@id", connection))
        {
            AddClientParams(command, c);
            command.Parameters.AddWithValue("id", c.Id);
            command.ExecuteNonQuery();
            return c.Id;
        }
    }

    private static void AddClientParams(NpgsqlCommand command, Client c)
    {
        command.Parameters.AddWithValue("ln", c.LastName);
        command.Parameters.AddWithValue("fn", c.FirstName);
        command.Parameters.AddWithValue("mn", (object?)c.MiddleName ?? DBNull.Value);
        command.Parameters.AddWithValue("phone", c.Phone);
        command.Parameters.AddWithValue("email", (object?)c.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("city", (object?)c.City ?? DBNull.Value);
        command.Parameters.AddWithValue("addr", (object?)c.Address ?? DBNull.Value);
        command.Parameters.AddWithValue("card", (object?)c.DiscountCard ?? DBNull.Value);
    }

    public static void DeleteClient(int id)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand("DELETE FROM client WHERE id=@id", connection);
        command.Parameters.AddWithValue("id", id);
        command.ExecuteNonQuery();
    }

    // ======================= ПРОДАЖИ =======================
    public static List<Sale> GetSales(string search = "", string status = "")
    {
        var list = new List<Sale>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT sa.id, sa.client_id,
                     cl.last_name || ' ' || cl.first_name || COALESCE(' ' || cl.middle_name,'') AS client_name,
                     sa.manager_id, u.last_name || ' ' || u.first_name AS manager_name,
                     sa.sale_date, sa.total_amount, sa.discount, sa.status, sa.payment_method,
                     COALESCE((SELECT SUM(p.amount) FROM payment p WHERE p.sale_id = sa.id), 0) AS paid,
                     (SELECT count(*) FROM sale_item si WHERE si.sale_id = sa.id) AS items_count
              FROM sale sa
              JOIN client cl ON cl.id = sa.client_id
              JOIN app_user u ON u.id = sa.manager_id
              WHERE (@s = '' OR cl.last_name ILIKE '%' || @s || '%' OR cl.first_name ILIKE '%' || @s || '%'
                     OR u.last_name ILIKE '%' || @s || '%')
                AND (@status = '' OR sa.status = @status)
              ORDER BY sa.sale_date DESC, sa.id DESC", connection);
        command.Parameters.AddWithValue("s", search);
        command.Parameters.AddWithValue("status", status);
        using var reader = command.ExecuteReader();
        while (reader.Read()) list.Add(ReadSale(reader));
        return list;
    }

    public static Sale? GetSale(int id)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT sa.id, sa.client_id,
                     cl.last_name || ' ' || cl.first_name || COALESCE(' ' || cl.middle_name,'') AS client_name,
                     sa.manager_id, u.last_name || ' ' || u.first_name AS manager_name,
                     sa.sale_date, sa.total_amount, sa.discount, sa.status, sa.payment_method,
                     COALESCE((SELECT SUM(p.amount) FROM payment p WHERE p.sale_id = sa.id), 0) AS paid,
                     (SELECT count(*) FROM sale_item si WHERE si.sale_id = sa.id) AS items_count
              FROM sale sa
              JOIN client cl ON cl.id = sa.client_id
              JOIN app_user u ON u.id = sa.manager_id
              WHERE sa.id = @id", connection);
        command.Parameters.AddWithValue("id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadSale(reader) : null;
    }

    private static Sale ReadSale(NpgsqlDataReader r) => new()
    {
        Id = r.GetInt32(0), ClientId = r.GetInt32(1), ClientName = r.GetString(2), ManagerId = r.GetInt32(3),
        ManagerName = r.GetString(4), SaleDate = r.GetDateTime(5), TotalAmount = r.GetDecimal(6),
        Discount = r.GetDecimal(7), Status = r.GetString(8), PaymentMethod = r.GetString(9),
        PaidAmount = r.GetDecimal(10), ItemsCount = Convert.ToInt32(r.GetInt64(11))
    };

    public static List<SaleItem> GetSaleItems(int saleId)
    {
        var list = new List<SaleItem>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT si.id, si.sale_id, si.product_id, p.name, p.sku, si.quantity, si.unit_price, si.total_price
              FROM sale_item si JOIN product p ON p.id = si.product_id
              WHERE si.sale_id = @id ORDER BY si.id", connection);
        command.Parameters.AddWithValue("id", saleId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(new SaleItem
            {
                Id = reader.GetInt32(0), SaleId = reader.GetInt32(1), ProductId = reader.GetInt32(2),
                ProductName = reader.GetString(3), Sku = reader.GetString(4), Quantity = reader.GetInt32(5),
                UnitPrice = reader.GetDecimal(6), TotalPrice = reader.GetDecimal(7)
            });
        return list;
    }

    public static int SaveSale(Sale sale, List<SaleItem> items)
    {
        using var connection = CreateConnection();
        using var transaction = connection.BeginTransaction();
        try
        {
            var total = items.Sum(i => i.Quantity * i.UnitPrice) - sale.Discount;
            if (total < 0) total = 0;

            if (sale.Id == 0)
            {
                using var command = new NpgsqlCommand(
                    @"INSERT INTO sale (client_id, manager_id, sale_date, total_amount, discount, status, payment_method)
                      VALUES (@cl,@mgr,@date,@total,@discount,@status,@method) RETURNING id", connection, transaction);
                AddSaleParams(command, sale, total);
                sale.Id = Convert.ToInt32(command.ExecuteScalar());
            }
            else
            {
                using var command = new NpgsqlCommand(
                    @"UPDATE sale SET client_id=@cl, manager_id=@mgr, sale_date=@date, total_amount=@total,
                           discount=@discount, status=@status, payment_method=@method WHERE id=@id",
                    connection, transaction);
                AddSaleParams(command, sale, total);
                command.Parameters.AddWithValue("id", sale.Id);
                command.ExecuteNonQuery();

                using var delete = new NpgsqlCommand("DELETE FROM sale_item WHERE sale_id=@id", connection, transaction);
                delete.Parameters.AddWithValue("id", sale.Id);
                delete.ExecuteNonQuery();
            }

            foreach (var item in items)
            {
                using var insert = new NpgsqlCommand(
                    @"INSERT INTO sale_item (sale_id, product_id, quantity, unit_price, total_price)
                      VALUES (@sale,@product,@qty,@price,@total)", connection, transaction);
                insert.Parameters.AddWithValue("sale", sale.Id);
                insert.Parameters.AddWithValue("product", item.ProductId);
                insert.Parameters.AddWithValue("qty", item.Quantity);
                insert.Parameters.AddWithValue("price", item.UnitPrice);
                insert.Parameters.AddWithValue("total", item.Quantity * item.UnitPrice);
                insert.ExecuteNonQuery();
            }

            sale.TotalAmount = total;
            transaction.Commit();
            return sale.Id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static void AddSaleParams(NpgsqlCommand command, Sale sale, decimal total)
    {
        command.Parameters.AddWithValue("cl", sale.ClientId);
        command.Parameters.AddWithValue("mgr", sale.ManagerId);
        command.Parameters.AddWithValue("date", sale.SaleDate);
        command.Parameters.AddWithValue("total", total);
        command.Parameters.AddWithValue("discount", sale.Discount);
        command.Parameters.AddWithValue("status", sale.Status);
        command.Parameters.AddWithValue("method", sale.PaymentMethod);
    }

    public static void DeleteSale(int id)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand("DELETE FROM sale WHERE id=@id", connection);
        command.Parameters.AddWithValue("id", id);
        command.ExecuteNonQuery();
    }

    // ======================= ПЛАТЕЖИ =======================
    public static List<Payment> GetPayments(int saleId)
    {
        var list = new List<Payment>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            "SELECT id, sale_id, amount, payment_method, payment_date, purpose FROM payment WHERE sale_id=@id ORDER BY payment_date",
            connection);
        command.Parameters.AddWithValue("id", saleId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(new Payment
            {
                Id = reader.GetInt32(0), SaleId = reader.GetInt32(1), Amount = reader.GetDecimal(2),
                PaymentMethod = reader.GetString(3), PaymentDate = reader.GetDateTime(4),
                Purpose = reader.IsDBNull(5) ? null : reader.GetString(5)
            });
        return list;
    }

    public static int SavePayment(Payment p)
    {
        using var connection = CreateConnection();
        if (p.Id == 0)
        {
            using var command = new NpgsqlCommand(
                @"INSERT INTO payment (sale_id, amount, payment_method, payment_date, purpose)
                  VALUES (@sale,@amount,@method,@date,@purpose) RETURNING id", connection);
            AddPaymentParams(command, p);
            return Convert.ToInt32(command.ExecuteScalar());
        }
        using (var command = new NpgsqlCommand(
                   @"UPDATE payment SET amount=@amount, payment_method=@method, payment_date=@date,
                          purpose=@purpose WHERE id=@id", connection))
        {
            AddPaymentParams(command, p);
            command.Parameters.AddWithValue("id", p.Id);
            command.ExecuteNonQuery();
            return p.Id;
        }
    }

    private static void AddPaymentParams(NpgsqlCommand command, Payment p)
    {
        command.Parameters.AddWithValue("sale", p.SaleId);
        command.Parameters.AddWithValue("amount", p.Amount);
        command.Parameters.AddWithValue("method", p.PaymentMethod);
        command.Parameters.AddWithValue("date", p.PaymentDate);
        command.Parameters.AddWithValue("purpose", (object?)p.Purpose ?? DBNull.Value);
    }

    public static void DeletePayment(int id)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand("DELETE FROM payment WHERE id=@id", connection);
        command.Parameters.AddWithValue("id", id);
        command.ExecuteNonQuery();
    }

    // ======================= ПОСТАВКИ =======================
    public static List<Supply> GetSupplies(string search = "", string status = "")
    {
        var list = new List<Supply>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT su.id, su.supplier_id, s.name, su.manager_id, u.last_name || ' ' || u.first_name,
                     su.invoice_number, su.supply_date, su.total_cost, su.status,
                     (SELECT count(*) FROM supply_item si WHERE si.supply_id = su.id) AS items_count
              FROM supply su
              JOIN supplier s ON s.id = su.supplier_id
              JOIN app_user u ON u.id = su.manager_id
              WHERE (@s = '' OR s.name ILIKE '%' || @s || '%' OR su.invoice_number ILIKE '%' || @s || '%')
                AND (@status = '' OR su.status = @status)
              ORDER BY su.supply_date DESC, su.id DESC", connection);
        command.Parameters.AddWithValue("s", search);
        command.Parameters.AddWithValue("status", status);
        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(new Supply
            {
                Id = reader.GetInt32(0), SupplierId = reader.GetInt32(1), SupplierName = reader.GetString(2),
                ManagerId = reader.GetInt32(3), ManagerName = reader.GetString(4),
                InvoiceNumber = reader.GetString(5), SupplyDate = reader.GetDateTime(6),
                TotalCost = reader.GetDecimal(7), Status = reader.GetString(8),
                ItemsCount = Convert.ToInt32(reader.GetInt64(9))
            });
        return list;
    }

    public static Supply? GetSupply(int id)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT su.id, su.supplier_id, s.name, su.manager_id, u.last_name || ' ' || u.first_name,
                     su.invoice_number, su.supply_date, su.total_cost, su.status,
                     (SELECT count(*) FROM supply_item si WHERE si.supply_id = su.id) AS items_count
              FROM supply su
              JOIN supplier s ON s.id = su.supplier_id
              JOIN app_user u ON u.id = su.manager_id
              WHERE su.id = @id", connection);
        command.Parameters.AddWithValue("id", id);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;
        return new Supply
        {
            Id = reader.GetInt32(0), SupplierId = reader.GetInt32(1), SupplierName = reader.GetString(2),
            ManagerId = reader.GetInt32(3), ManagerName = reader.GetString(4),
            InvoiceNumber = reader.GetString(5), SupplyDate = reader.GetDateTime(6),
            TotalCost = reader.GetDecimal(7), Status = reader.GetString(8),
            ItemsCount = Convert.ToInt32(reader.GetInt64(9))
        };
    }

    public static List<SupplyItem> GetSupplyItems(int supplyId)
    {
        var list = new List<SupplyItem>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT si.id, si.supply_id, si.product_id, p.name, p.sku, si.quantity,
                     si.purchase_price, si.total_price
              FROM supply_item si JOIN product p ON p.id = si.product_id
              WHERE si.supply_id = @id ORDER BY si.id", connection);
        command.Parameters.AddWithValue("id", supplyId);
        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(new SupplyItem
            {
                Id = reader.GetInt32(0), SupplyId = reader.GetInt32(1), ProductId = reader.GetInt32(2),
                ProductName = reader.GetString(3), Sku = reader.GetString(4), Quantity = reader.GetInt32(5),
                PurchasePrice = reader.GetDecimal(6), TotalPrice = reader.GetDecimal(7)
            });
        return list;
    }

    public static int SaveSupply(Supply supply, List<SupplyItem> items)
    {
        using var connection = CreateConnection();
        using var transaction = connection.BeginTransaction();
        try
        {
            var total = items.Sum(i => i.Quantity * i.PurchasePrice);
            if (supply.Id == 0)
            {
                using var command = new NpgsqlCommand(
                    @"INSERT INTO supply (supplier_id, manager_id, invoice_number, supply_date, total_cost, status)
                      VALUES (@sup,@mgr,@invoice,@date,@total,@status) RETURNING id", connection, transaction);
                AddSupplyParams(command, supply, total);
                supply.Id = Convert.ToInt32(command.ExecuteScalar());
            }
            else
            {
                using var command = new NpgsqlCommand(
                    @"UPDATE supply SET supplier_id=@sup, manager_id=@mgr, invoice_number=@invoice,
                           supply_date=@date, total_cost=@total, status=@status WHERE id=@id",
                    connection, transaction);
                AddSupplyParams(command, supply, total);
                command.Parameters.AddWithValue("id", supply.Id);
                command.ExecuteNonQuery();

                using var delete = new NpgsqlCommand("DELETE FROM supply_item WHERE supply_id=@id", connection, transaction);
                delete.Parameters.AddWithValue("id", supply.Id);
                delete.ExecuteNonQuery();
            }

            foreach (var item in items)
            {
                using var insert = new NpgsqlCommand(
                    @"INSERT INTO supply_item (supply_id, product_id, quantity, purchase_price, total_price)
                      VALUES (@supply,@product,@qty,@price,@total)", connection, transaction);
                insert.Parameters.AddWithValue("supply", supply.Id);
                insert.Parameters.AddWithValue("product", item.ProductId);
                insert.Parameters.AddWithValue("qty", item.Quantity);
                insert.Parameters.AddWithValue("price", item.PurchasePrice);
                insert.Parameters.AddWithValue("total", item.Quantity * item.PurchasePrice);
                insert.ExecuteNonQuery();
            }

            supply.TotalCost = total;
            transaction.Commit();
            return supply.Id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static void AddSupplyParams(NpgsqlCommand command, Supply supply, decimal total)
    {
        command.Parameters.AddWithValue("sup", supply.SupplierId);
        command.Parameters.AddWithValue("mgr", supply.ManagerId);
        command.Parameters.AddWithValue("invoice", supply.InvoiceNumber);
        command.Parameters.AddWithValue("date", supply.SupplyDate);
        command.Parameters.AddWithValue("total", total);
        command.Parameters.AddWithValue("status", supply.Status);
    }

    public static void DeleteSupply(int id)
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand("DELETE FROM supply WHERE id=@id", connection);
        command.Parameters.AddWithValue("id", id);
        command.ExecuteNonQuery();
    }

    // ======================= АНАЛИТИКА =======================
    public static DashboardStats GetDashboardStats()
    {
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT (SELECT count(*) FROM product),
                     (SELECT count(*) FROM sale),
                     (SELECT count(*) FROM client),
                     (SELECT COALESCE(SUM(total_amount),0) FROM sale WHERE status <> 'Отменена'),
                     (SELECT count(*) FROM sale WHERE status IN ('Новая','Оплачена','Собрана'))",
            connection);
        using var reader = command.ExecuteReader();
        reader.Read();
        return new DashboardStats
        {
            ProductsCount = reader.GetInt32(0), SalesCount = reader.GetInt32(1),
            ClientsCount = reader.GetInt32(2), Revenue = reader.GetDecimal(3),
            ActiveSales = reader.GetInt32(4)
        };
    }

    public static List<StatusCount> GetSalesByStatus()
    {
        var list = new List<StatusCount>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            "SELECT status, count(*) FROM sale GROUP BY status ORDER BY count(*) DESC", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(new StatusCount { Status = reader.GetString(0), Count = reader.GetInt32(1) });
        var max = Math.Max(1, list.Count == 0 ? 1 : list.Max(x => x.Count));
        foreach (var item in list) item.BarHeight = Math.Max(8, 160.0 * item.Count / max);
        return list;
    }

    public static List<Product> GetLowStock(int limit = 5)
    {
        var list = new List<Product>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT p.id, p.name, p.category_id, c.name, p.supplier_id, s.name, p.manufacturer,
                     p.model, p.sku, p.purchase_price, p.retail_price, p.warranty_months,
                     p.description, p.full_description, p.image_path, p.old_price, p.rating, p.reviews_count, p.min_stock, p.is_active, COALESCE(ps.stock, 0)
              FROM product p
              JOIN category c ON c.id = p.category_id
              JOIN supplier s ON s.id = p.supplier_id
              LEFT JOIN product_stock ps ON ps.product_id = p.id
              WHERE COALESCE(ps.stock, 0) <= @limit
              ORDER BY ps.stock, p.name", connection);
        command.Parameters.AddWithValue("limit", limit);
        using var reader = command.ExecuteReader();
        while (reader.Read()) list.Add(ReadProduct(reader));
        return list;
    }

    public static List<Sale> GetRecentSales(int count) => GetSales().Take(count).ToList();

    public static List<CategoryTotal> GetSalesByCategory(DateTime from, DateTime to)
    {
        var list = new List<CategoryTotal>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT c.name, count(DISTINCT sa.id), COALESCE(SUM(si.quantity),0), COALESCE(SUM(si.total_price),0)
              FROM sale sa
              JOIN sale_item si ON si.sale_id = sa.id
              JOIN product p ON p.id = si.product_id
              JOIN category c ON c.id = p.category_id
              WHERE sa.sale_date BETWEEN @f AND @t AND sa.status <> 'Отменена'
              GROUP BY c.name ORDER BY SUM(si.total_price) DESC", connection);
        command.Parameters.AddWithValue("f", from);
        command.Parameters.AddWithValue("t", to);
        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(new CategoryTotal
            {
                CategoryName = reader.GetString(0), SalesCount = Convert.ToInt32(reader.GetInt64(1)),
                ItemsCount = Convert.ToInt32(reader.GetInt64(2)), TotalSum = reader.GetDecimal(3)
            });
        return list;
    }

    public static List<ProductTotal> GetTopProducts(DateTime from, DateTime to)
    {
        var list = new List<ProductTotal>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT p.name, SUM(si.quantity), SUM(si.total_price)
              FROM sale_item si
              JOIN sale sa ON sa.id = si.sale_id
              JOIN product p ON p.id = si.product_id
              WHERE sa.sale_date BETWEEN @f AND @t AND sa.status <> 'Отменена'
              GROUP BY p.name ORDER BY SUM(si.total_price) DESC LIMIT 10", connection);
        command.Parameters.AddWithValue("f", from);
        command.Parameters.AddWithValue("t", to);
        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(new ProductTotal
            {
                ProductName = reader.GetString(0), Quantity = Convert.ToInt32(reader.GetDecimal(1)),
                TotalSum = reader.GetDecimal(2)
            });
        return list;
    }

    public static List<ClientTotal> GetTopClients(DateTime from, DateTime to)
    {
        var list = new List<ClientTotal>();
        using var connection = CreateConnection();
        using var command = new NpgsqlCommand(
            @"SELECT cl.last_name || ' ' || cl.first_name, count(*), COALESCE(SUM(sa.total_amount),0)
              FROM sale sa JOIN client cl ON cl.id = sa.client_id
              WHERE sa.sale_date BETWEEN @f AND @t AND sa.status <> 'Отменена'
              GROUP BY cl.last_name, cl.first_name ORDER BY SUM(sa.total_amount) DESC LIMIT 10", connection);
        command.Parameters.AddWithValue("f", from);
        command.Parameters.AddWithValue("t", to);
        using var reader = command.ExecuteReader();
        while (reader.Read())
            list.Add(new ClientTotal
            {
                ClientName = reader.GetString(0), SalesCount = Convert.ToInt32(reader.GetInt64(1)),
                TotalSum = reader.GetDecimal(2)
            });
        return list;
    }
}
