namespace NexaCommerce.Common.Constants;

public static class StoredProcedure
{
    // Users
    public const string UsersGet = "User_Get";
    public const string UsersGetByEmail = "User_GetByEmail";
    public const string UsersGetAll = "User_GetAll";
    public const string UsersInsertUpdate = "User_InsertUpdate";
    public const string UsersSoftDelete = "User_SoftDelete";
    public const string UsersIncrementAccessFailed = "User_IncrementAccessFailed";
    public const string UsersResetAccessFailed = "User_ResetAccessFailed";

    // Roles
    public const string RolesGet = "Role_Get";
    public const string RolesGetByName = "Role_GetByName";
    public const string RolesGetAll = "Role_GetAll";
    public const string RolesInsertUpdate = "Role_InsertUpdate";
    public const string RolesSoftDelete = "Role_SoftDelete";

    // UserRoles
    public const string UserRolesAssign = "UserRole_Assign";
    public const string UserRolesRemove = "UserRole_Remove";
    public const string UserRolesGetByUserId = "UserRole_GetByUserId";

    // RefreshTokens
    public const string RefreshTokensGet = "RefreshToken_Get";
    public const string RefreshTokensInsertUpdate = "RefreshToken_InsertUpdate";
    public const string RefreshTokensRevoke = "RefreshToken_Revoke";
    public const string RefreshTokensDeleteExpired = "RefreshToken_DeleteExpired";

    // UserSessions
    public const string UserSessionsGet = "UserSession_Get";
    public const string UserSessionsInsertUpdate = "UserSession_InsertUpdate";
    public const string UserSessionsRevokeAll = "UserSession_RevokeAll";

    // AuditLogs
    public const string AuditLogsGetAll = "AuditLog_GetAll";
    public const string AuditLogsInsertUpdate = "AuditLog_InsertUpdate";

    // SecurityEvents
    public const string SecurityEventsInsertUpdate = "SecurityEvent_InsertUpdate";

    // OutboxMessages
    public const string OutboxMessagesGet = "OutboxMessage_Get";
    public const string OutboxMessagesInsertUpdate = "OutboxMessage_InsertUpdate";

    // IdempotencyRecords
    public const string IdempotencyRecordsGet = "IdempotencyRecord_Get";
    public const string IdempotencyRecordsInsertUpdate = "IdempotencyRecord_InsertUpdate";

    // Vendors (Sellers / Stores)
    public const string VendorsGet = "Vendor_Get";
    public const string VendorsGetById = "Vendor_Get";
    public const string VendorsGetByUserId = "Vendor_GetByUserId";
    public const string VendorsGetAll = "Vendor_GetAll";
    public const string VendorsInsertUpdate = "Vendor_InsertUpdate";
    public const string VendorsUpdateStatus = "Vendor_UpdateStatus";

    // Categories
    public const string CategoriesGet = "Category_Get";
    public const string CategoriesGetBySlug = "Category_GetBySlug";
    public const string CategoriesGetAll = "Category_GetAll";
    public const string CategoriesInsertUpdate = "Category_InsertUpdate";
    public const string CategoriesSoftDelete = "Category_SoftDelete";

    // Brands
    public const string BrandsGet = "Brand_Get";
    public const string BrandsGetBySlug = "Brand_GetBySlug";
    public const string BrandsGetAll = "Brand_GetAll";
    public const string BrandsInsertUpdate = "Brand_InsertUpdate";
    public const string BrandsSoftDelete = "Brand_SoftDelete";

    // Products
    public const string ProductsGet = "Product_Get";
    public const string ProductsGetBySlug = "Product_GetBySlug";
    public const string ProductsGetAll = "Product_GetAll";
    public const string ProductsGetByVendorId = "Product_GetByVendorId";
    public const string ProductsGetForModeration = "Product_GetForModeration";
    public const string ProductsInsertUpdate = "Product_InsertUpdate";
    public const string ProductsUpdateStatus = "Product_UpdateStatus";
    public const string ProductsSoftDelete = "Product_SoftDelete";

    // Product Images
    public const string ProductImagesGetByProductId = "ProductImage_GetByProductId";
    public const string ProductImagesInsertUpdate = "ProductImage_InsertUpdate";
    public const string ProductImagesSetPrimary = "ProductImage_SetPrimary";
    public const string ProductImagesDelete = "ProductImage_Delete";

    // Product Variants
    public const string ProductVariantsGetByProductId = "ProductVariant_GetByProductId";
    public const string ProductVariantsInsertUpdate = "ProductVariant_InsertUpdate";
    public const string ProductVariantsDelete = "ProductVariant_Delete";
}

