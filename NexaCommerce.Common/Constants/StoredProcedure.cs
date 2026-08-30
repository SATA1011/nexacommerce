namespace NexaCommerce.Common.Constants;

public static class StoredProcedure
{
    // Users
    public const string UsersGet = "users_get";
    public const string UsersGetByEmail = "users_get_by_email";
    public const string UsersGetAll = "users_get_all";
    public const string UsersInsertUpdate = "users_insert_update";
    public const string UsersSoftDelete = "users_soft_delete";
    public const string UsersIncrementAccessFailed = "users_increment_access_failed";
    public const string UsersResetAccessFailed = "users_reset_access_failed";

    // Roles
    public const string RolesGet = "roles_get";
    public const string RolesGetAll = "roles_get_all";
    public const string RolesInsertUpdate = "roles_insert_update";
    public const string RolesSoftDelete = "roles_soft_delete";

    // UserRoles
    public const string UserRolesAssign = "user_roles_assign";
    public const string UserRolesRemove = "user_roles_remove";
    public const string UserRolesGetByUserId = "user_roles_get_by_user_id";

    // RefreshTokens
    public const string RefreshTokensGet = "refresh_tokens_get";
    public const string RefreshTokensInsertUpdate = "refresh_tokens_insert_update";
    public const string RefreshTokensRevoke = "refresh_tokens_revoke";
    public const string RefreshTokensDeleteExpired = "refresh_tokens_delete_expired";

    // UserSessions
    public const string UserSessionsGet = "user_sessions_get";
    public const string UserSessionsInsertUpdate = "user_sessions_insert_update";
    public const string UserSessionsRevokeAll = "user_sessions_revoke_all";

    // AuditLogs
    public const string AuditLogsGetAll = "audit_logs_get_all";
    public const string AuditLogsInsertUpdate = "audit_logs_insert_update";

    // SecurityEvents
    public const string SecurityEventsInsertUpdate = "security_events_insert_update";

    // OutboxMessages
    public const string OutboxMessagesGet = "outbox_messages_get";
    public const string OutboxMessagesInsertUpdate = "outbox_messages_insert_update";

    // IdempotencyRecords
    public const string IdempotencyRecordsGet = "idempotency_records_get";
    public const string IdempotencyRecordsInsertUpdate = "idempotency_records_insert_update";
}
