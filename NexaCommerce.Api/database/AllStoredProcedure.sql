-- NexaCommerce Database Stored Procedures
-- Database-First Architecture Specification (MySQL 8.0)
-- Pattern: <tablename>_<action>
-- All insert_update procedures accept a single `p_json` payload and return the inserted/updated row via SELECT

USE `nexacommerce`;

DELIMITER //

-- =============================================================================
-- 1. `users` Table Stored Procedures
-- =============================================================================

-- Get User By Email (Excludes soft-deleted)
DROP PROCEDURE IF EXISTS `User_GetByEmail` //
CREATE PROCEDURE `User_GetByEmail`(
    IN `p_email` VARCHAR(256)
)
BEGIN
    SELECT 
        u.`id`,
        u.`email`,
        u.`normalized_email`,
        u.`first_name`,
        u.`last_name`,
        u.`password_hash`,
        u.`phone_number`,
        u.`is_active`,
        u.`is_email_confirmed`,
        u.`is_deleted`,
        u.`security_stamp`,
        u.`two_factor_enabled`,
        u.`lockout_end_utc`,
        u.`lockout_enabled`,
        u.`access_failed_count`,
        u.`created_at_utc`,
        u.`updated_at_utc`,
        u.`last_login_at_utc`
    FROM `users` u
    WHERE u.`normalized_email` = UPPER(`p_email`) AND u.`is_deleted` = 0;
END //

-- Get User By Id (Excludes soft-deleted)
DROP PROCEDURE IF EXISTS `User_Get` //
CREATE PROCEDURE `User_Get`(
    IN `p_id` CHAR(36)
)
BEGIN
    SELECT 
        u.`id`,
        u.`email`,
        u.`normalized_email`,
        u.`first_name`,
        u.`last_name`,
        u.`password_hash`,
        u.`phone_number`,
        u.`is_active`,
        u.`is_email_confirmed`,
        u.`is_deleted`,
        u.`security_stamp`,
        u.`two_factor_enabled`,
        u.`lockout_end_utc`,
        u.`lockout_enabled`,
        u.`access_failed_count`,
        u.`created_at_utc`,
        u.`updated_at_utc`,
        u.`last_login_at_utc`
    FROM `users` u
    WHERE u.`id` = `p_id` AND u.`is_deleted` = 0;
END //

-- Get All Users (Paginated, Search Filter, Total Count, Excludes soft-deleted)
DROP PROCEDURE IF EXISTS `User_GetAll` //
CREATE PROCEDURE `User_GetAll`(
    IN `p_search_term` VARCHAR(256),
    IN `p_page_number` INT,
    IN `p_page_size` INT
)
BEGIN
    DECLARE `v_offset` INT;
    IF `p_page_number` IS NULL OR `p_page_number` < 1 THEN SET `p_page_number` = 1; END IF;
    IF `p_page_size` IS NULL OR `p_page_size` < 1 THEN SET `p_page_size` = 10; END IF;
    SET `v_offset` = (`p_page_number` - 1) * `p_page_size`;

    -- Result Set 1: Total Count
    SELECT COUNT(*) AS `total_count`
    FROM `users` u
    WHERE u.`is_deleted` = 0
      AND (`p_search_term` IS NULL OR `p_search_term` = '' 
           OR u.`email` LIKE CONCAT('%', `p_search_term`, '%')
           OR u.`first_name` LIKE CONCAT('%', `p_search_term`, '%')
           OR u.`last_name` LIKE CONCAT('%', `p_search_term`, '%'));

    -- Result Set 2: Paginated Data
    SELECT 
        u.`id`,
        u.`email`,
        u.`normalized_email`,
        u.`first_name`,
        u.`last_name`,
        u.`password_hash`,
        u.`phone_number`,
        u.`is_active`,
        u.`is_email_confirmed`,
        u.`is_deleted`,
        u.`security_stamp`,
        u.`two_factor_enabled`,
        u.`lockout_end_utc`,
        u.`lockout_enabled`,
        u.`access_failed_count`,
        u.`created_at_utc`,
        u.`updated_at_utc`,
        u.`last_login_at_utc`
    FROM `users` u
    WHERE u.`is_deleted` = 0
      AND (`p_search_term` IS NULL OR `p_search_term` = '' 
           OR u.`email` LIKE CONCAT('%', `p_search_term`, '%')
           OR u.`first_name` LIKE CONCAT('%', `p_search_term`, '%')
           OR u.`last_name` LIKE CONCAT('%', `p_search_term`, '%'))
    ORDER BY u.`created_at_utc` DESC
    LIMIT `p_page_size` OFFSET `v_offset`;
END //

-- Insert or Update User via JSON Payload (Returns inserted/updated record)
DROP PROCEDURE IF EXISTS `User_InsertUpdate` //
CREATE PROCEDURE `User_InsertUpdate`(
    IN `p_json` LONGTEXT
)
BEGIN
    DECLARE `v_id` CHAR(36);
    DECLARE `v_email` VARCHAR(256);
    DECLARE `v_first_name` VARCHAR(100);
    DECLARE `v_last_name` VARCHAR(100);
    DECLARE `v_password_hash` VARCHAR(500);
    DECLARE `v_phone_number` VARCHAR(30);
    DECLARE `v_security_stamp` VARCHAR(256);
    DECLARE `v_is_active` TINYINT(1);
    DECLARE `v_is_email_confirmed` TINYINT(1);

    SET `v_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.id'));
    SET `v_email` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.email'));
    SET `v_first_name` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.first_name'));
    SET `v_last_name` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.last_name'));
    SET `v_password_hash` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.password_hash'));
    SET `v_phone_number` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.phone_number'));
    SET `v_security_stamp` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.security_stamp'));
    SET `v_is_active` = COALESCE(JSON_EXTRACT(`p_json`, '$.is_active'), 1);
    SET `v_is_email_confirmed` = COALESCE(JSON_EXTRACT(`p_json`, '$.is_email_confirmed'), 0);

    INSERT INTO `users` (
        `id`,
        `email`,
        `normalized_email`,
        `first_name`,
        `last_name`,
        `password_hash`,
        `phone_number`,
        `is_active`,
        `is_email_confirmed`,
        `is_deleted`,
        `security_stamp`,
        `two_factor_enabled`,
        `lockout_enabled`,
        `access_failed_count`,
        `created_at_utc`,
        `updated_at_utc`
    ) VALUES (
        `v_id`,
        `v_email`,
        UPPER(`v_email`),
        `v_first_name`,
        `v_last_name`,
        `v_password_hash`,
        IF(`v_phone_number` = 'null' OR `v_phone_number` = '', NULL, `v_phone_number`),
        `v_is_active`,
        `v_is_email_confirmed`,
        0,
        IF(`v_security_stamp` = 'null' OR `v_security_stamp` = '', UUID(), `v_security_stamp`),
        0,
        1,
        0,
        NOW(6),
        NOW(6)
    )
    ON DUPLICATE KEY UPDATE
        `email` = `v_email`,
        `normalized_email` = UPPER(`v_email`),
        `first_name` = `v_first_name`,
        `last_name` = `v_last_name`,
        `password_hash` = `v_password_hash`,
        `phone_number` = IF(`v_phone_number` = 'null' OR `v_phone_number` = '', NULL, `v_phone_number`),
        `is_active` = `v_is_active`,
        `is_email_confirmed` = `v_is_email_confirmed`,
        `updated_at_utc` = NOW(6);

    -- Return full inserted / updated record
    SELECT 
        u.`id`,
        u.`email`,
        u.`normalized_email`,
        u.`first_name`,
        u.`last_name`,
        u.`password_hash`,
        u.`phone_number`,
        u.`is_active`,
        u.`is_email_confirmed`,
        u.`is_deleted`,
        u.`security_stamp`,
        u.`two_factor_enabled`,
        u.`lockout_end_utc`,
        u.`lockout_enabled`,
        u.`access_failed_count`,
        u.`created_at_utc`,
        u.`updated_at_utc`,
        u.`last_login_at_utc`
    FROM `users` u
    WHERE u.`id` = `v_id`;
END //

-- Soft Delete User
DROP PROCEDURE IF EXISTS `User_SoftDelete` //
CREATE PROCEDURE `User_SoftDelete`(
    IN `p_id` CHAR(36)
)
BEGIN
    UPDATE `users`
    SET `is_deleted` = 1,
        `is_active` = 0,
        `updated_at_utc` = NOW(6)
    WHERE `id` = `p_id`;
END //

-- Increment Access Failed Count (Security Lockout)
DROP PROCEDURE IF EXISTS `User_IncrementAccessFailed` //
CREATE PROCEDURE `User_IncrementAccessFailed`(
    IN `p_user_id` CHAR(36)
)
BEGIN
    UPDATE `users`
    SET `access_failed_count` = `access_failed_count` + 1,
        `lockout_end_utc` = CASE WHEN `access_failed_count` + 1 >= 5 THEN DATE_ADD(NOW(6), INTERVAL 15 MINUTE) ELSE `lockout_end_utc` END
    WHERE `id` = `p_user_id`;
END //

-- Reset Access Failed Count
DROP PROCEDURE IF EXISTS `User_ResetAccessFailed` //
CREATE PROCEDURE `User_ResetAccessFailed`(
    IN `p_user_id` CHAR(36)
)
BEGIN
    UPDATE `users`
    SET `access_failed_count` = 0,
        `lockout_end_utc` = NULL
    WHERE `id` = `p_user_id`;
END //

-- =============================================================================
-- 2. `roles` Table Stored Procedures
-- =============================================================================

-- Get Role By Id (Excludes soft-deleted)
DROP PROCEDURE IF EXISTS `Role_Get` //
CREATE PROCEDURE `Role_Get`(
    IN `p_id` CHAR(36)
)
BEGIN
    SELECT `id`, `name`, `normalized_name`, `description`, `is_deleted`
    FROM `roles`
    WHERE `id` = `p_id` AND `is_deleted` = 0;
END //

-- Get Role By Name (Case-insensitive, excludes soft-deleted)
DROP PROCEDURE IF EXISTS `Role_GetByName` //
CREATE PROCEDURE `Role_GetByName`(
    IN `p_name` VARCHAR(100)
)
BEGIN
    SELECT `id`, `name`, `normalized_name`, `description`, `is_deleted`
    FROM `roles`
    WHERE (`normalized_name` = UPPER(TRIM(`p_name`)) OR `name` = TRIM(`p_name`))
      AND `is_deleted` = 0
    LIMIT 1;
END //

-- Get All Roles (Paginated, Search Filter, Total Count, Excludes soft-deleted)
DROP PROCEDURE IF EXISTS `Role_GetAll` //
CREATE PROCEDURE `Role_GetAll`(
    IN `p_search_term` VARCHAR(256),
    IN `p_page_number` INT,
    IN `p_page_size` INT
)
BEGIN
    DECLARE `v_offset` INT;
    IF `p_page_number` IS NULL OR `p_page_number` < 1 THEN SET `p_page_number` = 1; END IF;
    IF `p_page_size` IS NULL OR `p_page_size` < 1 THEN SET `p_page_size` = 10; END IF;
    SET `v_offset` = (`p_page_number` - 1) * `p_page_size`;

    -- Result Set 1: Total Count
    SELECT COUNT(*) AS `total_count`
    FROM `roles`
    WHERE `is_deleted` = 0
      AND (`p_search_term` IS NULL OR `p_search_term` = '' OR `name` LIKE CONCAT('%', `p_search_term`, '%'));

    -- Result Set 2: Data
    SELECT `id`, `name`, `normalized_name`, `description`, `is_deleted`
    FROM `roles`
    WHERE `is_deleted` = 0
      AND (`p_search_term` IS NULL OR `p_search_term` = '' OR `name` LIKE CONCAT('%', `p_search_term`, '%'))
    ORDER BY `name` ASC
    LIMIT `p_page_size` OFFSET `v_offset`;
END //

-- Insert or Update Role via JSON Payload (Returns inserted/updated record)
DROP PROCEDURE IF EXISTS `Role_InsertUpdate` //
CREATE PROCEDURE `Role_InsertUpdate`(
    IN `p_json` LONGTEXT
)
BEGIN
    DECLARE `v_id` CHAR(36);
    DECLARE `v_name` VARCHAR(100);
    DECLARE `v_description` VARCHAR(256);

    SET `v_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.id'));
    SET `v_name` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.name'));
    SET `v_description` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.description'));

    INSERT INTO `roles` (`id`, `name`, `normalized_name`, `description`, `is_deleted`)
    VALUES (`v_id`, `v_name`, UPPER(`v_name`), `v_description`, 0)
    ON DUPLICATE KEY UPDATE
        `name` = `v_name`,
        `normalized_name` = UPPER(`v_name`),
        `description` = `v_description`;

    SELECT `id`, `name`, `normalized_name`, `description`, `is_deleted`
    FROM `roles`
    WHERE `id` = `v_id`;
END //

-- Soft Delete Role
DROP PROCEDURE IF EXISTS `Role_SoftDelete` //
CREATE PROCEDURE `Role_SoftDelete`(
    IN `p_id` CHAR(36)
)
BEGIN
    UPDATE `roles`
    SET `is_deleted` = 1
    WHERE `id` = `p_id`;
END //

-- =============================================================================
-- 3. `user_roles` Association Procedures
-- =============================================================================

-- Assign Role To User
DROP PROCEDURE IF EXISTS `UserRole_Assign` //
CREATE PROCEDURE `UserRole_Assign`(
    IN `p_user_id` CHAR(36),
    IN `p_role_id` CHAR(36)
)
BEGIN
    DECLARE `v_role_name` VARCHAR(100);
    SELECT `name` INTO `v_role_name` FROM `roles` WHERE `id` = `p_role_id`;

    INSERT INTO `user_roles` (`user_id`, `role_id`, `role_name`)
    VALUES (`p_user_id`, `p_role_id`, `v_role_name`)
    ON DUPLICATE KEY UPDATE `role_name` = VALUES(`role_name`);
END //

-- Remove Role From User
DROP PROCEDURE IF EXISTS `UserRole_Remove` //
CREATE PROCEDURE `UserRole_Remove`(
    IN `p_user_id` CHAR(36),
    IN `p_role_id` CHAR(36)
)
BEGIN
    DELETE FROM `user_roles`
    WHERE `user_id` = `p_user_id` AND `role_id` = `p_role_id`;
END //

-- Get Roles By User Id (For JWT Claims)
DROP PROCEDURE IF EXISTS `UserRole_GetByUserId` //
CREATE PROCEDURE `UserRole_GetByUserId`(
    IN `p_user_id` CHAR(36)
)
BEGIN
    SELECT r.`id`, r.`name`, r.`normalized_name`
    FROM `roles` r
    INNER JOIN `user_roles` ur ON r.`id` = ur.`role_id`
    WHERE ur.`user_id` = `p_user_id` AND r.`is_deleted` = 0;
END //

-- =============================================================================
-- 4. `refresh_tokens` Stored Procedures
-- =============================================================================

-- Get Refresh Token By Hash
DROP PROCEDURE IF EXISTS `RefreshToken_Get` //
CREATE PROCEDURE `RefreshToken_Get`(
    IN `p_token_hash` VARCHAR(500)
)
BEGIN
    SELECT 
        rt.`id`,
        rt.`user_id`,
        rt.`token_hash`,
        rt.`expires_at_utc`,
        rt.`created_by_ip`,
        rt.`created_at_utc`,
        rt.`revoked_at_utc`,
        rt.`revoked_by_ip`,
        rt.`replaced_by_token_hash`,
        rt.`reason_revoked`
    FROM `refresh_tokens` rt
    WHERE rt.`token_hash` = `p_token_hash`;
END //

-- Insert or Update Refresh Token via JSON Payload (Returns inserted/updated record)
DROP PROCEDURE IF EXISTS `RefreshToken_InsertUpdate` //
CREATE PROCEDURE `RefreshToken_InsertUpdate`(
    IN `p_json` LONGTEXT
)
BEGIN
    DECLARE `v_id` CHAR(36);
    DECLARE `v_user_id` CHAR(36);
    DECLARE `v_token_hash` VARCHAR(500);
    DECLARE `v_expires_at_utc` DATETIME(6);
    DECLARE `v_created_by_ip` VARCHAR(45);

    SET `v_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.id'));
    SET `v_user_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.user_id'));
    SET `v_token_hash` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.token_hash'));
    SET `v_expires_at_utc` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.expires_at_utc'));
    SET `v_created_by_ip` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.created_by_ip'));

    INSERT INTO `refresh_tokens` (
        `id`,
        `user_id`,
        `token_hash`,
        `expires_at_utc`,
        `created_by_ip`,
        `created_at_utc`
    ) VALUES (
        `v_id`,
        `v_user_id`,
        `v_token_hash`,
        `v_expires_at_utc`,
        `v_created_by_ip`,
        NOW(6)
    )
    ON DUPLICATE KEY UPDATE
        `expires_at_utc` = `v_expires_at_utc`,
        `created_by_ip` = `v_created_by_ip`;

    SELECT `id`, `user_id`, `token_hash`, `expires_at_utc`, `created_by_ip`, `created_at_utc`, `revoked_at_utc`, `revoked_by_ip`, `replaced_by_token_hash`, `reason_revoked`
    FROM `refresh_tokens`
    WHERE `id` = `v_id`;
END //

-- Revoke Refresh Token
DROP PROCEDURE IF EXISTS `RefreshToken_Revoke` //
CREATE PROCEDURE `RefreshToken_Revoke`(
    IN `p_token_hash` VARCHAR(500),
    IN `p_revoked_by_ip` VARCHAR(45),
    IN `p_replaced_by_token_hash` VARCHAR(500),
    IN `p_reason_revoked` VARCHAR(256)
)
BEGIN
    UPDATE `refresh_tokens`
    SET `revoked_at_utc` = NOW(6),
        `revoked_by_ip` = `p_revoked_by_ip`,
        `replaced_by_token_hash` = `p_replaced_by_token_hash`,
        `reason_revoked` = `p_reason_revoked`
    WHERE `token_hash` = `p_token_hash`;
END //

-- Delete Expired Refresh Tokens (Cleanup Background Worker)
DROP PROCEDURE IF EXISTS `RefreshToken_DeleteExpired` //
CREATE PROCEDURE `RefreshToken_DeleteExpired`()
BEGIN
    DELETE FROM `refresh_tokens`
    WHERE `expires_at_utc` < NOW(6);
END //

-- =============================================================================
-- 5. `user_sessions` Stored Procedures
-- =============================================================================

-- Get Active Sessions By User Id
DROP PROCEDURE IF EXISTS `UserSession_Get` //
CREATE PROCEDURE `UserSession_Get`(
    IN `p_user_id` CHAR(36)
)
BEGIN
    SELECT `id`, `user_id`, `device_name`, `ip_address`, `user_agent`, `created_at_utc`, `last_activity_at_utc`, `is_revoked`
    FROM `user_sessions`
    WHERE `user_id` = `p_user_id` AND `is_revoked` = 0;
END //

-- Insert or Update User Session via JSON Payload (Returns inserted/updated record)
DROP PROCEDURE IF EXISTS `UserSession_InsertUpdate` //
CREATE PROCEDURE `UserSession_InsertUpdate`(
    IN `p_json` LONGTEXT
)
BEGIN
    DECLARE `v_id` CHAR(36);
    DECLARE `v_user_id` CHAR(36);
    DECLARE `v_device_name` VARCHAR(256);
    DECLARE `v_ip_address` VARCHAR(45);
    DECLARE `v_user_agent` VARCHAR(500);

    SET `v_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.id'));
    SET `v_user_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.user_id'));
    SET `v_device_name` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.device_name'));
    SET `v_ip_address` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.ip_address'));
    SET `v_user_agent` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.user_agent'));

    INSERT INTO `user_sessions` (
        `id`, `user_id`, `device_name`, `ip_address`, `user_agent`, `created_at_utc`, `last_activity_at_utc`, `is_revoked`
    ) VALUES (
        `v_id`, `v_user_id`, `v_device_name`, `v_ip_address`, `v_user_agent`, NOW(6), NOW(6), 0
    )
    ON DUPLICATE KEY UPDATE
        `last_activity_at_utc` = NOW(6),
        `ip_address` = `v_ip_address`,
        `user_agent` = `v_user_agent`;

    SELECT `id`, `user_id`, `device_name`, `ip_address`, `user_agent`, `created_at_utc`, `last_activity_at_utc`, `is_revoked`
    FROM `user_sessions`
    WHERE `id` = `v_id`;
END //

-- Revoke All Sessions For User
DROP PROCEDURE IF EXISTS `UserSession_RevokeAll` //
CREATE PROCEDURE `UserSession_RevokeAll`(
    IN `p_user_id` CHAR(36)
)
BEGIN
    UPDATE `user_sessions`
    SET `is_revoked` = 1
    WHERE `user_id` = `p_user_id`;
END //

-- =============================================================================
-- 6. `security_events` & `audit_logs` Procedures
-- =============================================================================

-- Insert Security Event via JSON Payload (Returns inserted record)
DROP PROCEDURE IF EXISTS `SecurityEvent_InsertUpdate` //
CREATE PROCEDURE `SecurityEvent_InsertUpdate`(
    IN `p_json` LONGTEXT
)
BEGIN
    DECLARE `v_id` CHAR(36);
    DECLARE `v_user_id` CHAR(36);
    DECLARE `v_event_type` VARCHAR(100);
    DECLARE `v_ip_address` VARCHAR(45);
    DECLARE `v_user_agent` VARCHAR(500);
    DECLARE `v_details_json` LONGTEXT;

    SET `v_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.id'));
    SET `v_user_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.user_id'));
    SET `v_event_type` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.event_type'));
    SET `v_ip_address` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.ip_address'));
    SET `v_user_agent` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.user_agent'));
    SET `v_details_json` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.details_json'));

    INSERT INTO `security_events` (
        `id`, `user_id`, `event_type`, `ip_address`, `user_agent`, `details_json`, `created_at_utc`
    ) VALUES (
        `v_id`, `v_user_id`, `v_event_type`, `v_ip_address`, `v_user_agent`, `v_details_json`, NOW(6)
    );

    SELECT `id`, `user_id`, `event_type`, `ip_address`, `user_agent`, `details_json`, `created_at_utc`
    FROM `security_events`
    WHERE `id` = `v_id`;
END //

-- Insert Audit Log via JSON Payload (Returns inserted record)
DROP PROCEDURE IF EXISTS `AuditLog_InsertUpdate` //
CREATE PROCEDURE `AuditLog_InsertUpdate`(
    IN `p_json` LONGTEXT
)
BEGIN
    DECLARE `v_id` CHAR(36);
    DECLARE `v_user_id` CHAR(36);
    DECLARE `v_action` VARCHAR(100);
    DECLARE `v_entity_name` VARCHAR(100);
    DECLARE `v_entity_id` VARCHAR(100);
    DECLARE `v_old_values_json` LONGTEXT;
    DECLARE `v_new_values_json` LONGTEXT;
    DECLARE `v_ip_address` VARCHAR(45);
    DECLARE `v_user_agent` VARCHAR(500);

    SET `v_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.id'));
    SET `v_user_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.user_id'));
    SET `v_action` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.action'));
    SET `v_entity_name` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.entity_name'));
    SET `v_entity_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.entity_id'));
    SET `v_old_values_json` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.old_values_json'));
    SET `v_new_values_json` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.new_values_json'));
    SET `v_ip_address` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.ip_address'));
    SET `v_user_agent` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.user_agent'));

    INSERT INTO `audit_logs` (
        `id`, `user_id`, `action`, `entity_name`, `entity_id`, `old_values_json`, `new_values_json`, `ip_address`, `user_agent`, `created_at_utc`
    ) VALUES (
        `v_id`, `v_user_id`, `v_action`, `v_entity_name`, `v_entity_id`, `v_old_values_json`, `v_new_values_json`, `v_ip_address`, `v_user_agent`, NOW(6)
    );

    SELECT `id`, `user_id`, `action`, `entity_name`, `entity_id`, `old_values_json`, `new_values_json`, `ip_address`, `user_agent`, `created_at_utc`
    FROM `audit_logs`
    WHERE `id` = `v_id`;
END //

-- Get All Audit Logs (Paginated, Search Filter, Total Count)
DROP PROCEDURE IF EXISTS `AuditLog_GetAll` //
CREATE PROCEDURE `AuditLog_GetAll`(
    IN `p_search_term` VARCHAR(256),
    IN `p_page_number` INT,
    IN `p_page_size` INT
)
BEGIN
    DECLARE `v_offset` INT;
    IF `p_page_number` IS NULL OR `p_page_number` < 1 THEN SET `p_page_number` = 1; END IF;
    IF `p_page_size` IS NULL OR `p_page_size` < 1 THEN SET `p_page_size` = 10; END IF;
    SET `v_offset` = (`p_page_number` - 1) * `p_page_size`;

    -- Result Set 1: Total Count
    SELECT COUNT(*) AS `total_count`
    FROM `audit_logs`
    WHERE (`p_search_term` IS NULL OR `p_search_term` = '' 
           OR `action` LIKE CONCAT('%', `p_search_term`, '%')
           OR `entity_name` LIKE CONCAT('%', `p_search_term`, '%'));

    -- Result Set 2: Data
    SELECT `id`, `user_id`, `action`, `entity_name`, `entity_id`, `old_values_json`, `new_values_json`, `ip_address`, `user_agent`, `created_at_utc`
    FROM `audit_logs`
    WHERE (`p_search_term` IS NULL OR `p_search_term` = '' 
           OR `action` LIKE CONCAT('%', `p_search_term`, '%')
           OR `entity_name` LIKE CONCAT('%', `p_search_term`, '%'))
    ORDER BY `created_at_utc` DESC
    LIMIT `p_page_size` OFFSET `v_offset`;
END //

-- =============================================================================
-- 7. `outbox_messages` & `idempotency_records` Procedures
-- =============================================================================

-- Insert or Update Outbox Message via JSON Payload (Returns inserted/updated record)
DROP PROCEDURE IF EXISTS `OutboxMessage_InsertUpdate` //
CREATE PROCEDURE `OutboxMessage_InsertUpdate`(
    IN `p_json` LONGTEXT
)
BEGIN
    DECLARE `v_id` CHAR(36);
    DECLARE `v_type` VARCHAR(256);
    DECLARE `v_content` LONGTEXT;

    SET `v_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.id'));
    SET `v_type` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.type'));
    SET `v_content` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.content'));

    INSERT INTO `outbox_messages` (`id`, `type`, `content`, `occurred_on_utc`, `retry_count`)
    VALUES (`v_id`, `v_type`, `v_content`, NOW(6), 0)
    ON DUPLICATE KEY UPDATE
        `type` = `v_type`,
        `content` = `v_content`;

    SELECT `id`, `type`, `content`, `occurred_on_utc`, `processed_on_utc`, `error`, `retry_count`
    FROM `outbox_messages`
    WHERE `id` = `v_id`;
END //

-- Get Unprocessed Outbox Messages
DROP PROCEDURE IF EXISTS `OutboxMessage_Get` //
CREATE PROCEDURE `OutboxMessage_Get`(
    IN `p_batch_size` INT
)
BEGIN
    SELECT `id`, `type`, `content`, `occurred_on_utc`, `processed_on_utc`, `error`, `retry_count`
    FROM `outbox_messages`
    WHERE `processed_on_utc` IS NULL
    ORDER BY `occurred_on_utc` ASC
    LIMIT `p_batch_size`;
END //

-- Get Idempotency Record By Key
DROP PROCEDURE IF EXISTS `IdempotencyRecord_Get` //
CREATE PROCEDURE `IdempotencyRecord_Get`(
    IN `p_key` VARCHAR(256)
)
BEGIN
    SELECT `id`, `key`, `operation_name`, `request_hash`, `response_json`, `status_code`, `created_at_utc`, `expires_at_utc`
    FROM `idempotency_records`
    WHERE `key` = `p_key`;
END //

-- Insert or Update Idempotency Record via JSON Payload (Returns inserted/updated record)
DROP PROCEDURE IF EXISTS `IdempotencyRecord_InsertUpdate` //
CREATE PROCEDURE `IdempotencyRecord_InsertUpdate`(
    IN `p_json` LONGTEXT
)
BEGIN
    DECLARE `v_id` CHAR(36);
    DECLARE `v_key` VARCHAR(256);
    DECLARE `v_operation_name` VARCHAR(100);
    DECLARE `v_request_hash` VARCHAR(128);
    DECLARE `v_response_json` LONGTEXT;
    DECLARE `v_status_code` INT;
    DECLARE `v_expires_at_utc` DATETIME(6);

    SET `v_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.id'));
    SET `v_key` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.key'));
    SET `v_operation_name` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.operation_name'));
    SET `v_request_hash` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.request_hash'));
    SET `v_response_json` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.response_json'));
    SET `v_status_code` = JSON_EXTRACT(`p_json`, '$.status_code');
    SET `v_expires_at_utc` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.expires_at_utc'));

    INSERT INTO `idempotency_records` (
        `id`, `key`, `operation_name`, `request_hash`, `response_json`, `status_code`, `created_at_utc`, `expires_at_utc`
    ) VALUES (
        `v_id`, `v_key`, `v_operation_name`, `v_request_hash`, `v_response_json`, `v_status_code`, NOW(6), `v_expires_at_utc`
    )
    ON DUPLICATE KEY UPDATE
        `response_json` = `v_response_json`,
        `status_code` = `v_status_code`,
        `expires_at_utc` = `v_expires_at_utc`;

    SELECT `id`, `key`, `operation_name`, `request_hash`, `response_json`, `status_code`, `created_at_utc`, `expires_at_utc`
    FROM `idempotency_records`
    WHERE `id` = `v_id`;
END //

-- =============================================================================
-- 9. `vendors` Stored Procedures (Seller / Merchant Store Profiles)
-- =============================================================================

-- Get Vendor Store by ID
DROP PROCEDURE IF EXISTS `Vendor_GetById` //
DROP PROCEDURE IF EXISTS `Vendor_Get` //
CREATE PROCEDURE `Vendor_Get`(
    IN `p_id` CHAR(36)
)
BEGIN
    SELECT 
        c.`id`,
        c.`user_id`,
        c.`store_name`,
        c.`slug`,
        c.`description`,
        c.`tax_number`,
        c.`commission_rate`,
        c.`status`,
        c.`is_verified`,
        c.`created_at_utc`,
        c.`updated_at_utc`,
        c.`is_deleted`
    FROM `vendors` c
    WHERE c.`id` = `p_id` AND c.`is_deleted` = 0;
END //

-- Get Vendor Store by User ID
DROP PROCEDURE IF EXISTS `Vendor_GetByUserId` //
CREATE PROCEDURE `Vendor_GetByUserId`(
    IN `p_user_id` CHAR(36)
)
BEGIN
    SELECT 
        c.`id`,
        c.`user_id`,
        c.`store_name`,
        c.`slug`,
        c.`description`,
        c.`tax_number`,
        c.`commission_rate`,
        c.`status`,
        c.`is_verified`,
        c.`created_at_utc`,
        c.`updated_at_utc`,
        c.`is_deleted`
    FROM `vendors` c
    WHERE c.`user_id` = `p_user_id` AND c.`is_deleted` = 0;
END //

-- Get All Vendor Stores with Pagination, Search Term & Status Filter
DROP PROCEDURE IF EXISTS `Vendor_GetAll` //
CREATE PROCEDURE `Vendor_GetAll`(
    IN `p_search_term` VARCHAR(200),
    IN `p_status` VARCHAR(50),
    IN `p_page_number` INT,
    IN `p_page_size` INT
)
BEGIN
    DECLARE `v_offset` INT;
    SET `v_offset` = (`p_page_number` - 1) * `p_page_size`;

    -- Total Count
    SELECT COUNT(1)
    FROM `vendors` c
    WHERE c.`is_deleted` = 0
      AND (`p_search_term` IS NULL OR `p_search_term` = '' OR c.`store_name` LIKE CONCAT('%', `p_search_term`, '%') OR c.`slug` LIKE CONCAT('%', `p_search_term`, '%'))
      AND (`p_status` IS NULL OR `p_status` = '' OR c.`status` = `p_status`);

    -- Paginated Results
    SELECT 
        c.`id`,
        c.`user_id`,
        c.`store_name`,
        c.`slug`,
        c.`description`,
        c.`tax_number`,
        c.`commission_rate`,
        c.`status`,
        c.`is_verified`,
        c.`created_at_utc`,
        c.`updated_at_utc`,
        c.`is_deleted`
    FROM `vendors` c
    WHERE c.`is_deleted` = 0
      AND (`p_search_term` IS NULL OR `p_search_term` = '' OR c.`store_name` LIKE CONCAT('%', `p_search_term`, '%') OR c.`slug` LIKE CONCAT('%', `p_search_term`, '%'))
      AND (`p_status` IS NULL OR `p_status` = '' OR c.`status` = `p_status`)
    ORDER BY c.`created_at_utc` DESC
    LIMIT `v_offset`, `p_page_size`;
END //

-- Insert or Update Vendor Store via JSON Payload
DROP PROCEDURE IF EXISTS `Vendor_InsertUpdate` //
CREATE PROCEDURE `Vendor_InsertUpdate`(
    IN `p_json` LONGTEXT
)
BEGIN
    DECLARE `v_id` CHAR(36);
    DECLARE `v_user_id` CHAR(36);
    DECLARE `v_store_name` VARCHAR(200);
    DECLARE `v_slug` VARCHAR(200);
    DECLARE `v_description` TEXT;
    DECLARE `v_tax_number` VARCHAR(100);
    DECLARE `v_commission_rate` DECIMAL(5, 2);
    DECLARE `v_status` VARCHAR(50);
    DECLARE `v_is_verified` TINYINT(1);

    SET `v_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.id'));
    SET `v_user_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.user_id'));
    SET `v_store_name` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.store_name'));
    SET `v_slug` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.slug'));
    SET `v_description` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.description'));
    SET `v_tax_number` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.tax_number'));
    SET `v_commission_rate` = COALESCE(JSON_EXTRACT(`p_json`, '$.commission_rate'), 10.00);
    SET `v_status` = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.status')), 'Pending');
    SET `v_is_verified` = COALESCE(JSON_EXTRACT(`p_json`, '$.is_verified'), 0);

    INSERT INTO `vendors` (
        `id`, `user_id`, `store_name`, `slug`, `description`, `tax_number`, `commission_rate`, `status`, `is_verified`, `created_at_utc`, `is_deleted`
    ) VALUES (
        `v_id`, `v_user_id`, `v_store_name`, `v_slug`, `v_description`, `v_tax_number`, `v_commission_rate`, `v_status`, `v_is_verified`, UTC_TIMESTAMP(6), 0
    )
    ON DUPLICATE KEY UPDATE
        `store_name` = `v_store_name`,
        `slug` = `v_slug`,
        `description` = `v_description`,
        `tax_number` = `v_tax_number`,
        `commission_rate` = `v_commission_rate`,
        `status` = `v_status`,
        `is_verified` = `v_is_verified`,
        `updated_at_utc` = UTC_TIMESTAMP(6);

    SELECT 
        c.`id`,
        c.`user_id`,
        c.`store_name`,
        c.`slug`,
        c.`description`,
        c.`tax_number`,
        c.`commission_rate`,
        c.`status`,
        c.`is_verified`,
        c.`created_at_utc`,
        c.`updated_at_utc`,
        c.`is_deleted`
    FROM `vendors` c
    WHERE c.`id` = `v_id`;
END //

-- Update Vendor Store Status (Admin Approval / Rejection / Suspension)
DROP PROCEDURE IF EXISTS `Vendor_UpdateStatus` //
CREATE PROCEDURE `Vendor_UpdateStatus`(
    IN `p_id` CHAR(36),
    IN `p_status` VARCHAR(50),
    IN `p_is_verified` TINYINT(1)
)
BEGIN
    UPDATE `vendors`
    SET 
        `status` = `p_status`,
        `is_verified` = `p_is_verified`,
        `updated_at_utc` = UTC_TIMESTAMP(6)
    WHERE `id` = `p_id` AND `is_deleted` = 0;

    SELECT 
        c.`id`,
        c.`user_id`,
        c.`store_name`,
        c.`slug`,
        c.`description`,
        c.`tax_number`,
        c.`commission_rate`,
        c.`status`,
        c.`is_verified`,
        c.`created_at_utc`,
        c.`updated_at_utc`,
        c.`is_deleted`
    FROM `vendors` c
    WHERE c.`id` = `p_id`;
END //

-- =============================================================================
-- 10. `categories` Table Stored Procedures
-- =============================================================================

-- Get Category By Id
DROP PROCEDURE IF EXISTS `Category_Get` //
CREATE PROCEDURE `Category_Get`(
    IN `p_id` CHAR(36)
)
BEGIN
    SELECT 
        c.`id`, c.`parent_id`, c.`name`, c.`slug`, c.`description`, c.`image_url`,
        c.`display_order`, c.`is_active`, c.`is_deleted`, c.`created_at_utc`, c.`updated_at_utc`
    FROM `categories` c
    WHERE c.`id` = `p_id` AND c.`is_deleted` = 0;
END //

-- Get Category By Slug
DROP PROCEDURE IF EXISTS `Category_GetBySlug` //
CREATE PROCEDURE `Category_GetBySlug`(
    IN `p_slug` VARCHAR(150)
)
BEGIN
    SELECT 
        c.`id`, c.`parent_id`, c.`name`, c.`slug`, c.`description`, c.`image_url`,
        c.`display_order`, c.`is_active`, c.`is_deleted`, c.`created_at_utc`, c.`updated_at_utc`
    FROM `categories` c
    WHERE c.`slug` = `p_slug` AND c.`is_deleted` = 0;
END //

-- Get All Active Categories
DROP PROCEDURE IF EXISTS `Category_GetAll` //
CREATE PROCEDURE `Category_GetAll`()
BEGIN
    SELECT 
        c.`id`, c.`parent_id`, c.`name`, c.`slug`, c.`description`, c.`image_url`,
        c.`display_order`, c.`is_active`, c.`is_deleted`, c.`created_at_utc`, c.`updated_at_utc`
    FROM `categories` c
    WHERE c.`is_deleted` = 0 AND c.`is_active` = 1
    ORDER BY c.`display_order` ASC, c.`name` ASC;
END //

-- Insert or Update Category
DROP PROCEDURE IF EXISTS `Category_InsertUpdate` //
CREATE PROCEDURE `Category_InsertUpdate`(
    IN `p_json` LONGTEXT
)
BEGIN
    DECLARE `v_id` CHAR(36);
    DECLARE `v_parent_id` CHAR(36);
    DECLARE `v_name` VARCHAR(150);
    DECLARE `v_slug` VARCHAR(150);
    DECLARE `v_description` VARCHAR(500);
    DECLARE `v_image_url` VARCHAR(1000);
    DECLARE `v_display_order` INT;
    DECLARE `v_is_active` TINYINT(1);

    SET `v_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.id'));
    SET `v_parent_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.parent_id'));
    IF `v_parent_id` = 'null' OR `v_parent_id` = '' THEN SET `v_parent_id` = NULL; END IF;
    SET `v_name` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.name'));
    SET `v_slug` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.slug'));
    SET `v_description` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.description'));
    SET `v_image_url` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.image_url'));
    SET `v_display_order` = COALESCE(JSON_EXTRACT(`p_json`, '$.display_order'), 0);
    SET `v_is_active` = COALESCE(JSON_EXTRACT(`p_json`, '$.is_active'), 1);

    INSERT INTO `categories` (
        `id`, `parent_id`, `name`, `slug`, `description`, `image_url`, `display_order`, `is_active`, `is_deleted`, `created_at_utc`
    ) VALUES (
        `v_id`, `v_parent_id`, `v_name`, `v_slug`, `v_description`, `v_image_url`, `v_display_order`, `v_is_active`, 0, UTC_TIMESTAMP(6)
    )
    ON DUPLICATE KEY UPDATE
        `parent_id` = `v_parent_id`,
        `name` = `v_name`,
        `slug` = `v_slug`,
        `description` = `v_description`,
        `image_url` = `v_image_url`,
        `display_order` = `v_display_order`,
        `is_active` = `v_is_active`,
        `updated_at_utc` = UTC_TIMESTAMP(6);

    SELECT 
        c.`id`, c.`parent_id`, c.`name`, c.`slug`, c.`description`, c.`image_url`,
        c.`display_order`, c.`is_active`, c.`is_deleted`, c.`created_at_utc`, c.`updated_at_utc`
    FROM `categories` c
    WHERE c.`id` = `v_id`;
END //

-- Soft Delete Category
DROP PROCEDURE IF EXISTS `Category_SoftDelete` //
CREATE PROCEDURE `Category_SoftDelete`(
    IN `p_id` CHAR(36)
)
BEGIN
    UPDATE `categories`
    SET `is_deleted` = 1, `updated_at_utc` = UTC_TIMESTAMP(6)
    WHERE `id` = `p_id`;
END //

-- =============================================================================
-- 11. `brands` Table Stored Procedures
-- =============================================================================

-- Get Brand By Id
DROP PROCEDURE IF EXISTS `Brand_Get` //
CREATE PROCEDURE `Brand_Get`(
    IN `p_id` CHAR(36)
)
BEGIN
    SELECT 
        b.`id`, b.`name`, b.`slug`, b.`description`, b.`logo_url`,
        b.`is_active`, b.`is_deleted`, b.`created_at_utc`, b.`updated_at_utc`
    FROM `brands` b
    WHERE b.`id` = `p_id` AND b.`is_deleted` = 0;
END //

-- Get Brand By Slug
DROP PROCEDURE IF EXISTS `Brand_GetBySlug` //
CREATE PROCEDURE `Brand_GetBySlug`(
    IN `p_slug` VARCHAR(150)
)
BEGIN
    SELECT 
        b.`id`, b.`name`, b.`slug`, b.`description`, b.`logo_url`,
        b.`is_active`, b.`is_deleted`, b.`created_at_utc`, b.`updated_at_utc`
    FROM `brands` b
    WHERE b.`slug` = `p_slug` AND b.`is_deleted` = 0;
END //

-- Get All Active Brands
DROP PROCEDURE IF EXISTS `Brand_GetAll` //
CREATE PROCEDURE `Brand_GetAll`()
BEGIN
    SELECT 
        b.`id`, b.`name`, b.`slug`, b.`description`, b.`logo_url`,
        b.`is_active`, b.`is_deleted`, b.`created_at_utc`, b.`updated_at_utc`
    FROM `brands` b
    WHERE b.`is_deleted` = 0 AND b.`is_active` = 1
    ORDER BY b.`name` ASC;
END //

-- Insert or Update Brand
DROP PROCEDURE IF EXISTS `Brand_InsertUpdate` //
CREATE PROCEDURE `Brand_InsertUpdate`(
    IN `p_json` LONGTEXT
)
BEGIN
    DECLARE `v_id` CHAR(36);
    DECLARE `v_name` VARCHAR(150);
    DECLARE `v_slug` VARCHAR(150);
    DECLARE `v_description` VARCHAR(500);
    DECLARE `v_logo_url` VARCHAR(1000);
    DECLARE `v_is_active` TINYINT(1);

    SET `v_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.id'));
    SET `v_name` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.name'));
    SET `v_slug` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.slug'));
    SET `v_description` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.description'));
    SET `v_logo_url` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.logo_url'));
    SET `v_is_active` = COALESCE(JSON_EXTRACT(`p_json`, '$.is_active'), 1);

    INSERT INTO `brands` (
        `id`, `name`, `slug`, `description`, `logo_url`, `is_active`, `is_deleted`, `created_at_utc`
    ) VALUES (
        `v_id`, `v_name`, `v_slug`, `v_description`, `v_logo_url`, `v_is_active`, 0, UTC_TIMESTAMP(6)
    )
    ON DUPLICATE KEY UPDATE
        `name` = `v_name`,
        `slug` = `v_slug`,
        `description` = `v_description`,
        `logo_url` = `v_logo_url`,
        `is_active` = `v_is_active`,
        `updated_at_utc` = UTC_TIMESTAMP(6);

    SELECT 
        b.`id`, b.`name`, b.`slug`, b.`description`, b.`logo_url`,
        b.`is_active`, b.`is_deleted`, b.`created_at_utc`, b.`updated_at_utc`
    FROM `brands` b
    WHERE b.`id` = `v_id`;
END //

-- Soft Delete Brand
DROP PROCEDURE IF EXISTS `Brand_SoftDelete` //
CREATE PROCEDURE `Brand_SoftDelete`(
    IN `p_id` CHAR(36)
)
BEGIN
    UPDATE `brands`
    SET `is_deleted` = 1, `updated_at_utc` = UTC_TIMESTAMP(6)
    WHERE `id` = `p_id`;
END //

-- =============================================================================
-- 12. `products` Table Stored Procedures
-- =============================================================================

-- Get Product By Id (Includes Vendor, Category, Brand details)
DROP PROCEDURE IF EXISTS `Product_Get` //
CREATE PROCEDURE `Product_Get`(
    IN `p_id` CHAR(36)
)
BEGIN
    SELECT 
        p.`id`, p.`vendor_id`, p.`category_id`, p.`brand_id`, p.`title`, p.`slug`,
        p.`short_description`, p.`description`, p.`sku`, p.`price`, p.`compare_at_price`,
        p.`cost_price`, p.`stock_quantity`, p.`primary_image_url`, p.`status`, p.`rejection_reason`,
        p.`is_active`, p.`is_deleted`, p.`created_at_utc`, p.`updated_at_utc`,
        v.`store_name` AS `vendor_store_name`,
        v.`slug` AS `vendor_slug`,
        c.`name` AS `category_name`,
        b.`name` AS `brand_name`
    FROM `products` p
    INNER JOIN `vendors` v ON p.`vendor_id` = v.`id`
    LEFT JOIN `categories` c ON p.`category_id` = c.`id`
    LEFT JOIN `brands` b ON p.`brand_id` = b.`id`
    WHERE p.`id` = `p_id` AND p.`is_deleted` = 0;
END //

-- Get Product By Slug (Public Storefront Details)
DROP PROCEDURE IF EXISTS `Product_GetBySlug` //
CREATE PROCEDURE `Product_GetBySlug`(
    IN `p_slug` VARCHAR(255)
)
BEGIN
    SELECT 
        p.`id`, p.`vendor_id`, p.`category_id`, p.`brand_id`, p.`title`, p.`slug`,
        p.`short_description`, p.`description`, p.`sku`, p.`price`, p.`compare_at_price`,
        p.`cost_price`, p.`stock_quantity`, p.`primary_image_url`, p.`status`, p.`rejection_reason`,
        p.`is_active`, p.`is_deleted`, p.`created_at_utc`, p.`updated_at_utc`,
        v.`store_name` AS `vendor_store_name`,
        v.`slug` AS `vendor_slug`,
        c.`name` AS `category_name`,
        b.`name` AS `brand_name`
    FROM `products` p
    INNER JOIN `vendors` v ON p.`vendor_id` = v.`id`
    LEFT JOIN `categories` c ON p.`category_id` = c.`id`
    LEFT JOIN `brands` b ON p.`brand_id` = b.`id`
    WHERE p.`slug` = `p_slug` 
      AND p.`is_deleted` = 0 
      AND p.`status` = 'Approved' 
      AND p.`is_active` = 1
      AND v.`status` = 'Approved'
      AND v.`is_verified` = 1;
END //

-- Get All Products for Public Storefront (Search, Category, Brand, Price filters, Pagination)
DROP PROCEDURE IF EXISTS `Product_GetAll` //
CREATE PROCEDURE `Product_GetAll`(
    IN `p_search_term` VARCHAR(255),
    IN `p_category_id` CHAR(36),
    IN `p_brand_id` CHAR(36),
    IN `p_vendor_slug` VARCHAR(200),
    IN `p_min_price` DECIMAL(12, 2),
    IN `p_max_price` DECIMAL(12, 2),
    IN `p_sort_by` VARCHAR(50),
    IN `p_page_number` INT,
    IN `p_page_size` INT
)
BEGIN
    DECLARE `v_offset` INT;
    IF `p_page_number` IS NULL OR `p_page_number` < 1 THEN SET `p_page_number` = 1; END IF;
    IF `p_page_size` IS NULL OR `p_page_size` < 1 THEN SET `p_page_size` = 12; END IF;
    SET `v_offset` = (`p_page_number` - 1) * `p_page_size`;

    -- Result Set 1: Total Count
    SELECT COUNT(*) AS `total_count`
    FROM `products` p
    INNER JOIN `vendors` v ON p.`vendor_id` = v.`id`
    WHERE p.`is_deleted` = 0
      AND p.`status` = 'Approved'
      AND p.`is_active` = 1
      AND v.`status` = 'Approved'
      AND v.`is_verified` = 1
      AND v.`is_deleted` = 0
      AND (`p_search_term` IS NULL OR `p_search_term` = '' 
           OR p.`title` LIKE CONCAT('%', `p_search_term`, '%') 
           OR p.`description` LIKE CONCAT('%', `p_search_term`, '%')
           OR v.`store_name` LIKE CONCAT('%', `p_search_term`, '%'))
      AND (`p_category_id` IS NULL OR `p_category_id` = '' OR p.`category_id` = `p_category_id`)
      AND (`p_brand_id` IS NULL OR `p_brand_id` = '' OR p.`brand_id` = `p_brand_id`)
      AND (`p_vendor_slug` IS NULL OR `p_vendor_slug` = '' OR v.`slug` = `p_vendor_slug`)
      AND (`p_min_price` IS NULL OR p.`price` >= `p_min_price`)
      AND (`p_max_price` IS NULL OR p.`price` <= `p_max_price`);

    -- Result Set 2: Paginated Data
    SELECT 
        p.`id`, p.`vendor_id`, p.`category_id`, p.`brand_id`, p.`title`, p.`slug`,
        p.`short_description`, p.`sku`, p.`price`, p.`compare_at_price`,
        p.`stock_quantity`, p.`primary_image_url`, p.`status`,
        p.`is_active`, p.`created_at_utc`,
        v.`store_name` AS `vendor_store_name`,
        v.`slug` AS `vendor_slug`,
        c.`name` AS `category_name`,
        b.`name` AS `brand_name`
    FROM `products` p
    INNER JOIN `vendors` v ON p.`vendor_id` = v.`id`
    LEFT JOIN `categories` c ON p.`category_id` = c.`id`
    LEFT JOIN `brands` b ON p.`brand_id` = b.`id`
    WHERE p.`is_deleted` = 0
      AND p.`status` = 'Approved'
      AND p.`is_active` = 1
      AND v.`status` = 'Approved'
      AND v.`is_verified` = 1
      AND v.`is_deleted` = 0
      AND (`p_search_term` IS NULL OR `p_search_term` = '' 
           OR p.`title` LIKE CONCAT('%', `p_search_term`, '%') 
           OR p.`description` LIKE CONCAT('%', `p_search_term`, '%')
           OR v.`store_name` LIKE CONCAT('%', `p_search_term`, '%'))
      AND (`p_category_id` IS NULL OR `p_category_id` = '' OR p.`category_id` = `p_category_id`)
      AND (`p_brand_id` IS NULL OR `p_brand_id` = '' OR p.`brand_id` = `p_brand_id`)
      AND (`p_vendor_slug` IS NULL OR `p_vendor_slug` = '' OR v.`slug` = `p_vendor_slug`)
      AND (`p_min_price` IS NULL OR p.`price` >= `p_min_price`)
      AND (`p_max_price` IS NULL OR p.`price` <= `p_max_price`)
    ORDER BY 
      CASE WHEN `p_sort_by` = 'price_asc' THEN p.`price` END ASC,
      CASE WHEN `p_sort_by` = 'price_desc' THEN p.`price` END DESC,
      CASE WHEN `p_sort_by` = 'newest' THEN p.`created_at_utc` END DESC,
      p.`created_at_utc` DESC
    LIMIT `v_offset`, `p_page_size`;
END //

-- Get Products By Vendor Id (Vendor Studio Scoped Query)
DROP PROCEDURE IF EXISTS `Product_GetByVendorId` //
CREATE PROCEDURE `Product_GetByVendorId`(
    IN `p_vendor_id` CHAR(36),
    IN `p_search_term` VARCHAR(255),
    IN `p_status` VARCHAR(50),
    IN `p_page_number` INT,
    IN `p_page_size` INT
)
BEGIN
    DECLARE `v_offset` INT;
    IF `p_page_number` IS NULL OR `p_page_number` < 1 THEN SET `p_page_number` = 1; END IF;
    IF `p_page_size` IS NULL OR `p_page_size` < 1 THEN SET `p_page_size` = 10; END IF;
    SET `v_offset` = (`p_page_number` - 1) * `p_page_size`;

    -- Result Set 1: Total Count
    SELECT COUNT(*) AS `total_count`
    FROM `products` p
    WHERE p.`vendor_id` = `p_vendor_id`
      AND p.`is_deleted` = 0
      AND (`p_search_term` IS NULL OR `p_search_term` = '' 
           OR p.`title` LIKE CONCAT('%', `p_search_term`, '%') 
           OR p.`sku` LIKE CONCAT('%', `p_search_term`, '%'))
      AND (`p_status` IS NULL OR `p_status` = '' OR p.`status` = `p_status`);

    -- Result Set 2: Paginated Data
    SELECT 
        p.`id`, p.`vendor_id`, p.`category_id`, p.`brand_id`, p.`title`, p.`slug`,
        p.`short_description`, p.`description`, p.`sku`, p.`price`, p.`compare_at_price`,
        p.`cost_price`, p.`stock_quantity`, p.`primary_image_url`, p.`status`, p.`rejection_reason`,
        p.`is_active`, p.`is_deleted`, p.`created_at_utc`, p.`updated_at_utc`,
        c.`name` AS `category_name`,
        b.`name` AS `brand_name`
    FROM `products` p
    LEFT JOIN `categories` c ON p.`category_id` = c.`id`
    LEFT JOIN `brands` b ON p.`brand_id` = b.`id`
    WHERE p.`vendor_id` = `p_vendor_id`
      AND p.`is_deleted` = 0
      AND (`p_search_term` IS NULL OR `p_search_term` = '' 
           OR p.`title` LIKE CONCAT('%', `p_search_term`, '%') 
           OR p.`sku` LIKE CONCAT('%', `p_search_term`, '%'))
      AND (`p_status` IS NULL OR `p_status` = '' OR p.`status` = `p_status`)
    ORDER BY p.`created_at_utc` DESC
    LIMIT `v_offset`, `p_page_size`;
END //

-- Get Products For Admin Moderation
DROP PROCEDURE IF EXISTS `Product_GetForModeration` //
CREATE PROCEDURE `Product_GetForModeration`(
    IN `p_search_term` VARCHAR(255),
    IN `p_status` VARCHAR(50),
    IN `p_page_number` INT,
    IN `p_page_size` INT
)
BEGIN
    DECLARE `v_offset` INT;
    IF `p_page_number` IS NULL OR `p_page_number` < 1 THEN SET `p_page_number` = 1; END IF;
    IF `p_page_size` IS NULL OR `p_page_size` < 1 THEN SET `p_page_size` = 15; END IF;
    SET `v_offset` = (`p_page_number` - 1) * `p_page_size`;

    -- Result Set 1: Total Count
    SELECT COUNT(*) AS `total_count`
    FROM `products` p
    INNER JOIN `vendors` v ON p.`vendor_id` = v.`id`
    WHERE p.`is_deleted` = 0
      AND (`p_search_term` IS NULL OR `p_search_term` = '' 
           OR p.`title` LIKE CONCAT('%', `p_search_term`, '%')
           OR v.`store_name` LIKE CONCAT('%', `p_search_term`, '%'))
      AND (`p_status` IS NULL OR `p_status` = '' OR p.`status` = `p_status`);

    -- Result Set 2: Paginated Data
    SELECT 
        p.`id`, p.`vendor_id`, p.`category_id`, p.`brand_id`, p.`title`, p.`slug`,
        p.`short_description`, p.`sku`, p.`price`, p.`compare_at_price`,
        p.`stock_quantity`, p.`primary_image_url`, p.`status`, p.`rejection_reason`,
        p.`is_active`, p.`created_at_utc`, p.`updated_at_utc`,
        v.`store_name` AS `vendor_store_name`,
        v.`slug` AS `vendor_slug`,
        c.`name` AS `category_name`,
        b.`name` AS `brand_name`
    FROM `products` p
    INNER JOIN `vendors` v ON p.`vendor_id` = v.`id`
    LEFT JOIN `categories` c ON p.`category_id` = c.`id`
    LEFT JOIN `brands` b ON p.`brand_id` = b.`id`
    WHERE p.`is_deleted` = 0
      AND (`p_search_term` IS NULL OR `p_search_term` = '' 
           OR p.`title` LIKE CONCAT('%', `p_search_term`, '%')
           OR v.`store_name` LIKE CONCAT('%', `p_search_term`, '%'))
      AND (`p_status` IS NULL OR `p_status` = '' OR p.`status` = `p_status`)
    ORDER BY p.`created_at_utc` DESC
    LIMIT `v_offset`, `p_page_size`;
END //

-- Insert or Update Product via JSON Payload
DROP PROCEDURE IF EXISTS `Product_InsertUpdate` //
CREATE PROCEDURE `Product_InsertUpdate`(
    IN `p_json` LONGTEXT
)
BEGIN
    DECLARE `v_id` CHAR(36);
    DECLARE `v_vendor_id` CHAR(36);
    DECLARE `v_category_id` CHAR(36);
    DECLARE `v_brand_id` CHAR(36);
    DECLARE `v_title` VARCHAR(255);
    DECLARE `v_slug` VARCHAR(255);
    DECLARE `v_short_description` VARCHAR(500);
    DECLARE `v_description` LONGTEXT;
    DECLARE `v_sku` VARCHAR(100);
    DECLARE `v_price` DECIMAL(12, 2);
    DECLARE `v_compare_at_price` DECIMAL(12, 2);
    DECLARE `v_cost_price` DECIMAL(12, 2);
    DECLARE `v_stock_quantity` INT;
    DECLARE `v_primary_image_url` VARCHAR(1000);
    DECLARE `v_status` VARCHAR(50);
    DECLARE `v_is_active` TINYINT(1);

    SET `v_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.id'));
    SET `v_vendor_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.vendor_id'));
    SET `v_category_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.category_id'));
    IF `v_category_id` = 'null' OR `v_category_id` = '' THEN SET `v_category_id` = NULL; END IF;
    SET `v_brand_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.brand_id'));
    IF `v_brand_id` = 'null' OR `v_brand_id` = '' THEN SET `v_brand_id` = NULL; END IF;
    SET `v_title` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.title'));
    SET `v_slug` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.slug'));
    SET `v_short_description` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.short_description'));
    SET `v_description` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.description'));
    SET `v_sku` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.sku'));
    SET `v_price` = COALESCE(JSON_EXTRACT(`p_json`, '$.price'), 0.00);
    SET `v_compare_at_price` = JSON_EXTRACT(`p_json`, '$.compare_at_price');
    SET `v_cost_price` = JSON_EXTRACT(`p_json`, '$.cost_price');
    SET `v_stock_quantity` = COALESCE(JSON_EXTRACT(`p_json`, '$.stock_quantity'), 0);
    SET `v_primary_image_url` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.primary_image_url'));
    SET `v_status` = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.status')), 'Draft');
    SET `v_is_active` = COALESCE(JSON_EXTRACT(`p_json`, '$.is_active'), 1);

    INSERT INTO `products` (
        `id`, `vendor_id`, `category_id`, `brand_id`, `title`, `slug`,
        `short_description`, `description`, `sku`, `price`, `compare_at_price`,
        `cost_price`, `stock_quantity`, `primary_image_url`, `status`,
        `is_active`, `is_deleted`, `created_at_utc`
    ) VALUES (
        `v_id`, `v_vendor_id`, `v_category_id`, `v_brand_id`, `v_title`, `v_slug`,
        `v_short_description`, `v_description`, `v_sku`, `v_price`, `v_compare_at_price`,
        `v_cost_price`, `v_stock_quantity`, `v_primary_image_url`, `v_status`,
        `v_is_active`, 0, UTC_TIMESTAMP(6)
    )
    ON DUPLICATE KEY UPDATE
        `category_id` = `v_category_id`,
        `brand_id` = `v_brand_id`,
        `title` = `v_title`,
        `slug` = `v_slug`,
        `short_description` = `v_short_description`,
        `description` = `v_description`,
        `sku` = `v_sku`,
        `price` = `v_price`,
        `compare_at_price` = `v_compare_at_price`,
        `cost_price` = `v_cost_price`,
        `stock_quantity` = `v_stock_quantity`,
        `primary_image_url` = COALESCE(`v_primary_image_url`, `primary_image_url`),
        `status` = `v_status`,
        `is_active` = `v_is_active`,
        `updated_at_utc` = UTC_TIMESTAMP(6);

    CALL `Product_Get`(`v_id`);
END //

-- Update Product Status (Admin Moderation)
DROP PROCEDURE IF EXISTS `Product_UpdateStatus` //
CREATE PROCEDURE `Product_UpdateStatus`(
    IN `p_id` CHAR(36),
    IN `p_status` VARCHAR(50),
    IN `p_rejection_reason` VARCHAR(500)
)
BEGIN
    UPDATE `products`
    SET 
        `status` = `p_status`,
        `rejection_reason` = `p_rejection_reason`,
        `updated_at_utc` = UTC_TIMESTAMP(6)
    WHERE `id` = `p_id` AND `is_deleted` = 0;

    CALL `Product_Get`(`p_id`);
END //

-- Soft Delete Product
DROP PROCEDURE IF EXISTS `Product_SoftDelete` //
CREATE PROCEDURE `Product_SoftDelete`(
    IN `p_id` CHAR(36),
    IN `p_vendor_id` CHAR(36)
)
BEGIN
    UPDATE `products`
    SET `is_deleted` = 1, `updated_at_utc` = UTC_TIMESTAMP(6)
    WHERE `id` = `p_id` 
      AND (`p_vendor_id` IS NULL OR `vendor_id` = `p_vendor_id`);
END //

-- =============================================================================
-- 13. `product_images` Table Stored Procedures
-- =============================================================================

-- Get All Images for a Product (Ordered by Sort Order)
DROP PROCEDURE IF EXISTS `ProductImage_GetByProductId` //
CREATE PROCEDURE `ProductImage_GetByProductId`(
    IN `p_product_id` CHAR(36)
)
BEGIN
    SELECT 
        pi.`id`, pi.`product_id`, pi.`variant_id`, pi.`image_url`, pi.`thumbnail_url`,
        pi.`alt_text`, pi.`sort_order`, pi.`is_primary`, pi.`is_deleted`,
        pi.`created_at_utc`, pi.`updated_at_utc`
    FROM `product_images` pi
    WHERE pi.`product_id` = `p_product_id` AND pi.`is_deleted` = 0
    ORDER BY pi.`sort_order` ASC, pi.`created_at_utc` ASC;
END //

-- Insert or Update Product Image (Synchronizes primary image to `products` table)
DROP PROCEDURE IF EXISTS `ProductImage_InsertUpdate` //
CREATE PROCEDURE `ProductImage_InsertUpdate`(
    IN `p_json` LONGTEXT
)
BEGIN
    DECLARE `v_id` CHAR(36);
    DECLARE `v_product_id` CHAR(36);
    DECLARE `v_variant_id` CHAR(36);
    DECLARE `v_image_url` VARCHAR(1000);
    DECLARE `v_thumbnail_url` VARCHAR(1000);
    DECLARE `v_alt_text` VARCHAR(255);
    DECLARE `v_sort_order` INT;
    DECLARE `v_is_primary` TINYINT(1);

    SET `v_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.id'));
    SET `v_product_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.product_id'));
    SET `v_variant_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.variant_id'));
    IF `v_variant_id` = 'null' OR `v_variant_id` = '' THEN SET `v_variant_id` = NULL; END IF;
    SET `v_image_url` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.image_url'));
    SET `v_thumbnail_url` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.thumbnail_url'));
    SET `v_alt_text` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.alt_text'));
    SET `v_sort_order` = COALESCE(JSON_EXTRACT(`p_json`, '$.sort_order'), 0);
    SET `v_is_primary` = COALESCE(JSON_EXTRACT(`p_json`, '$.is_primary'), 0);

    -- If this is set as primary, unmark other images for this product
    IF `v_is_primary` = 1 THEN
        UPDATE `product_images`
        SET `is_primary` = 0
        WHERE `product_id` = `v_product_id`;

        -- Synchronize denormalized primary image URL on products table
        UPDATE `products`
        SET `primary_image_url` = `v_image_url`, `updated_at_utc` = UTC_TIMESTAMP(6)
        WHERE `id` = `v_product_id`;
    END IF;

    INSERT INTO `product_images` (
        `id`, `product_id`, `variant_id`, `image_url`, `thumbnail_url`,
        `alt_text`, `sort_order`, `is_primary`, `is_deleted`, `created_at_utc`
    ) VALUES (
        `v_id`, `v_product_id`, `v_variant_id`, `v_image_url`, `v_thumbnail_url`,
        `v_alt_text`, `v_sort_order`, `v_is_primary`, 0, UTC_TIMESTAMP(6)
    )
    ON DUPLICATE KEY UPDATE
        `variant_id` = `v_variant_id`,
        `image_url` = `v_image_url`,
        `thumbnail_url` = `v_thumbnail_url`,
        `alt_text` = `v_alt_text`,
        `sort_order` = `v_sort_order`,
        `is_primary` = `v_is_primary`,
        `updated_at_utc` = UTC_TIMESTAMP(6);

    SELECT 
        pi.`id`, pi.`product_id`, pi.`variant_id`, pi.`image_url`, pi.`thumbnail_url`,
        pi.`alt_text`, pi.`sort_order`, pi.`is_primary`, pi.`is_deleted`,
        pi.`created_at_utc`, pi.`updated_at_utc`
    FROM `product_images` pi
    WHERE pi.`id` = `v_id`;
END //

-- Set Specific Image as Primary and Sync to Products Table
DROP PROCEDURE IF EXISTS `ProductImage_SetPrimary` //
CREATE PROCEDURE `ProductImage_SetPrimary`(
    IN `p_id` CHAR(36),
    IN `p_product_id` CHAR(36)
)
BEGIN
    DECLARE `v_image_url` VARCHAR(1000);

    -- Unmark all
    UPDATE `product_images`
    SET `is_primary` = 0, `updated_at_utc` = UTC_TIMESTAMP(6)
    WHERE `product_id` = `p_product_id`;

    -- Mark selected
    UPDATE `product_images`
    SET `is_primary` = 1, `updated_at_utc` = UTC_TIMESTAMP(6)
    WHERE `id` = `p_id` AND `product_id` = `p_product_id`;

    -- Fetch target image url
    SELECT `image_url` INTO `v_image_url`
    FROM `product_images`
    WHERE `id` = `p_id` AND `product_id` = `p_product_id`;

    -- Update products table
    IF `v_image_url` IS NOT NULL THEN
        UPDATE `products`
        SET `primary_image_url` = `v_image_url`, `updated_at_utc` = UTC_TIMESTAMP(6)
        WHERE `id` = `p_product_id`;
    END IF;

    SELECT 
        pi.`id`, pi.`product_id`, pi.`variant_id`, pi.`image_url`, pi.`thumbnail_url`,
        pi.`alt_text`, pi.`sort_order`, pi.`is_primary`, pi.`is_deleted`,
        pi.`created_at_utc`, pi.`updated_at_utc`
    FROM `product_images` pi
    WHERE pi.`id` = `p_id`;
END //

-- Delete Product Image
DROP PROCEDURE IF EXISTS `ProductImage_Delete` //
CREATE PROCEDURE `ProductImage_Delete`(
    IN `p_id` CHAR(36),
    IN `p_product_id` CHAR(36)
)
BEGIN
    DECLARE `v_was_primary` TINYINT(1) DEFAULT 0;
    DECLARE `v_next_image_id` CHAR(36);
    DECLARE `v_next_image_url` VARCHAR(1000);

    SELECT `is_primary` INTO `v_was_primary`
    FROM `product_images`
    WHERE `id` = `p_id` AND `product_id` = `p_product_id`;

    -- Soft delete
    UPDATE `product_images`
    SET `is_deleted` = 1, `is_primary` = 0, `updated_at_utc` = UTC_TIMESTAMP(6)
    WHERE `id` = `p_id` AND `product_id` = `p_product_id`;

    -- If the deleted image was primary, select the first remaining active image
    IF `v_was_primary` = 1 THEN
        SELECT `id`, `image_url` INTO `v_next_image_id`, `v_next_image_url`
        FROM `product_images`
        WHERE `product_id` = `p_product_id` AND `is_deleted` = 0
        ORDER BY `sort_order` ASC, `created_at_utc` ASC
        LIMIT 1;

        IF `v_next_image_id` IS NOT NULL THEN
            UPDATE `product_images`
            SET `is_primary` = 1, `updated_at_utc` = UTC_TIMESTAMP(6)
            WHERE `id` = `v_next_image_id`;

            UPDATE `products`
            SET `primary_image_url` = `v_next_image_url`, `updated_at_utc` = UTC_TIMESTAMP(6)
            WHERE `id` = `p_product_id`;
        ELSE
            UPDATE `products`
            SET `primary_image_url` = NULL, `updated_at_utc` = UTC_TIMESTAMP(6)
            WHERE `id` = `p_product_id`;
        END IF;
    END IF;
END //

-- =============================================================================
-- 14. `product_variants` Table Stored Procedures
-- =============================================================================

-- Get Variants by Product Id
DROP PROCEDURE IF EXISTS `ProductVariant_GetByProductId` //
CREATE PROCEDURE `ProductVariant_GetByProductId`(
    IN `p_product_id` CHAR(36)
)
BEGIN
    SELECT 
        pv.`id`, pv.`product_id`, pv.`sku`, pv.`title`, pv.`price`,
        pv.`compare_at_price`, pv.`stock_quantity`, pv.`attributes_json`,
        pv.`is_active`, pv.`is_deleted`, pv.`created_at_utc`, pv.`updated_at_utc`
    FROM `product_variants` pv
    WHERE pv.`product_id` = `p_product_id` AND pv.`is_deleted` = 0;
END //

-- Insert or Update Product Variant
DROP PROCEDURE IF EXISTS `ProductVariant_InsertUpdate` //
CREATE PROCEDURE `ProductVariant_InsertUpdate`(
    IN `p_json` LONGTEXT
)
BEGIN
    DECLARE `v_id` CHAR(36);
    DECLARE `v_product_id` CHAR(36);
    DECLARE `v_sku` VARCHAR(100);
    DECLARE `v_title` VARCHAR(150);
    DECLARE `v_price` DECIMAL(12, 2);
    DECLARE `v_compare_at_price` DECIMAL(12, 2);
    DECLARE `v_stock_quantity` INT;
    DECLARE `v_attributes_json` LONGTEXT;
    DECLARE `v_is_active` TINYINT(1);

    SET `v_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.id'));
    SET `v_product_id` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.product_id'));
    SET `v_sku` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.sku'));
    SET `v_title` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.title'));
    SET `v_price` = COALESCE(JSON_EXTRACT(`p_json`, '$.price'), 0.00);
    SET `v_compare_at_price` = JSON_EXTRACT(`p_json`, '$.compare_at_price');
    SET `v_stock_quantity` = COALESCE(JSON_EXTRACT(`p_json`, '$.stock_quantity'), 0);
    SET `v_attributes_json` = JSON_UNQUOTE(JSON_EXTRACT(`p_json`, '$.attributes_json'));
    SET `v_is_active` = COALESCE(JSON_EXTRACT(`p_json`, '$.is_active'), 1);

    INSERT INTO `product_variants` (
        `id`, `product_id`, `sku`, `title`, `price`, `compare_at_price`,
        `stock_quantity`, `attributes_json`, `is_active`, `is_deleted`, `created_at_utc`
    ) VALUES (
        `v_id`, `v_product_id`, `v_sku`, `v_title`, `v_price`, `v_compare_at_price`,
        `v_stock_quantity`, `v_attributes_json`, `v_is_active`, 0, UTC_TIMESTAMP(6)
    )
    ON DUPLICATE KEY UPDATE
        `sku` = `v_sku`,
        `title` = `v_title`,
        `price` = `v_price`,
        `compare_at_price` = `v_compare_at_price`,
        `stock_quantity` = `v_stock_quantity`,
        `attributes_json` = `v_attributes_json`,
        `is_active` = `v_is_active`,
        `updated_at_utc` = UTC_TIMESTAMP(6);

    SELECT 
        pv.`id`, pv.`product_id`, pv.`sku`, pv.`title`, pv.`price`,
        pv.`compare_at_price`, pv.`stock_quantity`, pv.`attributes_json`,
        pv.`is_active`, pv.`is_deleted`, pv.`created_at_utc`, pv.`updated_at_utc`
    FROM `product_variants` pv
    WHERE pv.`id` = `v_id`;
END //

-- Delete Product Variant
DROP PROCEDURE IF EXISTS `ProductVariant_Delete` //
CREATE PROCEDURE `ProductVariant_Delete`(
    IN `p_id` CHAR(36),
    IN `p_product_id` CHAR(36)
)
BEGIN
    UPDATE `product_variants`
    SET `is_deleted` = 1, `updated_at_utc` = UTC_TIMESTAMP(6)
    WHERE `id` = `p_id` AND `product_id` = `p_product_id`;
END //

DELIMITER ;

