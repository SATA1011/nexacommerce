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
DROP PROCEDURE IF EXISTS `users_get_by_email` //
CREATE PROCEDURE `users_get_by_email`(
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
DROP PROCEDURE IF EXISTS `users_get` //
CREATE PROCEDURE `users_get`(
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
DROP PROCEDURE IF EXISTS `users_get_all` //
CREATE PROCEDURE `users_get_all`(
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
DROP PROCEDURE IF EXISTS `users_insert_update` //
CREATE PROCEDURE `users_insert_update`(
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
DROP PROCEDURE IF EXISTS `users_soft_delete` //
CREATE PROCEDURE `users_soft_delete`(
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
DROP PROCEDURE IF EXISTS `users_increment_access_failed` //
CREATE PROCEDURE `users_increment_access_failed`(
    IN `p_user_id` CHAR(36)
)
BEGIN
    UPDATE `users`
    SET `access_failed_count` = `access_failed_count` + 1,
        `lockout_end_utc` = CASE WHEN `access_failed_count` + 1 >= 5 THEN DATE_ADD(NOW(6), INTERVAL 15 MINUTE) ELSE `lockout_end_utc` END
    WHERE `id` = `p_user_id`;
END //

-- Reset Access Failed Count
DROP PROCEDURE IF EXISTS `users_reset_access_failed` //
CREATE PROCEDURE `users_reset_access_failed`(
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
DROP PROCEDURE IF EXISTS `roles_get` //
CREATE PROCEDURE `roles_get`(
    IN `p_id` CHAR(36)
)
BEGIN
    SELECT `id`, `name`, `normalized_name`, `description`, `is_deleted`
    FROM `roles`
    WHERE `id` = `p_id` AND `is_deleted` = 0;
END //

-- Get All Roles (Paginated, Search Filter, Total Count, Excludes soft-deleted)
DROP PROCEDURE IF EXISTS `roles_get_all` //
CREATE PROCEDURE `roles_get_all`(
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
DROP PROCEDURE IF EXISTS `roles_insert_update` //
CREATE PROCEDURE `roles_insert_update`(
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
DROP PROCEDURE IF EXISTS `roles_soft_delete` //
CREATE PROCEDURE `roles_soft_delete`(
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
DROP PROCEDURE IF EXISTS `user_roles_assign` //
CREATE PROCEDURE `user_roles_assign`(
    IN `p_user_id` CHAR(36),
    IN `p_role_id` CHAR(36)
)
BEGIN
    INSERT IGNORE INTO `user_roles` (`user_id`, `role_id`)
    VALUES (`p_user_id`, `p_role_id`);
END //

-- Remove Role From User
DROP PROCEDURE IF EXISTS `user_roles_remove` //
CREATE PROCEDURE `user_roles_remove`(
    IN `p_user_id` CHAR(36),
    IN `p_role_id` CHAR(36)
)
BEGIN
    DELETE FROM `user_roles`
    WHERE `user_id` = `p_user_id` AND `role_id` = `p_role_id`;
END //

-- Get Roles By User Id (For JWT Claims)
DROP PROCEDURE IF EXISTS `user_roles_get_by_user_id` //
CREATE PROCEDURE `user_roles_get_by_user_id`(
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
DROP PROCEDURE IF EXISTS `refresh_tokens_get` //
CREATE PROCEDURE `refresh_tokens_get`(
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
DROP PROCEDURE IF EXISTS `refresh_tokens_insert_update` //
CREATE PROCEDURE `refresh_tokens_insert_update`(
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
DROP PROCEDURE IF EXISTS `refresh_tokens_revoke` //
CREATE PROCEDURE `refresh_tokens_revoke`(
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
DROP PROCEDURE IF EXISTS `refresh_tokens_delete_expired` //
CREATE PROCEDURE `refresh_tokens_delete_expired`()
BEGIN
    DELETE FROM `refresh_tokens`
    WHERE `expires_at_utc` < NOW(6);
END //

-- =============================================================================
-- 5. `user_sessions` Stored Procedures
-- =============================================================================

-- Get Active Sessions By User Id
DROP PROCEDURE IF EXISTS `user_sessions_get` //
CREATE PROCEDURE `user_sessions_get`(
    IN `p_user_id` CHAR(36)
)
BEGIN
    SELECT `id`, `user_id`, `device_name`, `ip_address`, `user_agent`, `created_at_utc`, `last_activity_at_utc`, `is_revoked`
    FROM `user_sessions`
    WHERE `user_id` = `p_user_id` AND `is_revoked` = 0;
END //

-- Insert or Update User Session via JSON Payload (Returns inserted/updated record)
DROP PROCEDURE IF EXISTS `user_sessions_insert_update` //
CREATE PROCEDURE `user_sessions_insert_update`(
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
DROP PROCEDURE IF EXISTS `user_sessions_revoke_all` //
CREATE PROCEDURE `user_sessions_revoke_all`(
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
DROP PROCEDURE IF EXISTS `security_events_insert_update` //
CREATE PROCEDURE `security_events_insert_update`(
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
DROP PROCEDURE IF EXISTS `audit_logs_insert_update` //
CREATE PROCEDURE `audit_logs_insert_update`(
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
DROP PROCEDURE IF EXISTS `audit_logs_get_all` //
CREATE PROCEDURE `audit_logs_get_all`(
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
DROP PROCEDURE IF EXISTS `outbox_messages_insert_update` //
CREATE PROCEDURE `outbox_messages_insert_update`(
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
DROP PROCEDURE IF EXISTS `outbox_messages_get` //
CREATE PROCEDURE `outbox_messages_get`(
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
DROP PROCEDURE IF EXISTS `idempotency_records_get` //
CREATE PROCEDURE `idempotency_records_get`(
    IN `p_key` VARCHAR(256)
)
BEGIN
    SELECT `id`, `key`, `operation_name`, `request_hash`, `response_json`, `status_code`, `created_at_utc`, `expires_at_utc`
    FROM `idempotency_records`
    WHERE `key` = `p_key`;
END //

-- Insert or Update Idempotency Record via JSON Payload (Returns inserted/updated record)
DROP PROCEDURE IF EXISTS `idempotency_records_insert_update` //
CREATE PROCEDURE `idempotency_records_insert_update`(
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

DELIMITER ;
