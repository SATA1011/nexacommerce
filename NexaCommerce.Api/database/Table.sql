-- NexaCommerce Database Schema Script
-- Database-First Architecture Specification (MySQL 8.0 / InnoDB)
-- Clean Lowercase / snake_case Table Definitions with Soft Delete (`is_deleted`)

CREATE DATABASE IF NOT EXISTS `nexacommerce`;
USE `nexacommerce`;

-- =============================================================================
-- 1. Identity & Access Management Module
-- =============================================================================

-- Users Table
CREATE TABLE IF NOT EXISTS `users` (
    `id` CHAR(36) NOT NULL,
    `email` VARCHAR(256) NOT NULL,
    `normalized_email` VARCHAR(256) NOT NULL,
    `first_name` VARCHAR(100) NOT NULL,
    `last_name` VARCHAR(100) NOT NULL,
    `password_hash` VARCHAR(500) NOT NULL,
    `phone_number` VARCHAR(30) NULL,
    `is_active` TINYINT(1) NOT NULL DEFAULT 1,
    `is_email_confirmed` TINYINT(1) NOT NULL DEFAULT 0,
    `is_deleted` TINYINT(1) NOT NULL DEFAULT 0,
    `security_stamp` VARCHAR(256) NOT NULL,
    `two_factor_enabled` TINYINT(1) NOT NULL DEFAULT 0,
    `lockout_end_utc` DATETIME(6) NULL,
    `lockout_enabled` TINYINT(1) NOT NULL DEFAULT 1,
    `access_failed_count` INT NOT NULL DEFAULT 0,
    `created_at_utc` DATETIME(6) NOT NULL,
    `updated_at_utc` DATETIME(6) NULL,
    `last_login_at_utc` DATETIME(6) NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_users_email` (`email`),
    UNIQUE KEY `ux_users_normalized_email` (`normalized_email`),
    KEY `ix_users_is_deleted` (`is_deleted`)
) ENGINE=InnoDB;

-- Roles Table
CREATE TABLE IF NOT EXISTS `roles` (
    `id` CHAR(36) NOT NULL,
    `name` VARCHAR(100) NOT NULL,
    `normalized_name` VARCHAR(100) NOT NULL,
    `description` VARCHAR(256) NULL,
    `is_deleted` TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_roles_name` (`name`),
    UNIQUE KEY `ux_roles_normalized_name` (`normalized_name`),
    KEY `ix_roles_is_deleted` (`is_deleted`)
) ENGINE=InnoDB;

-- Permissions Table
CREATE TABLE IF NOT EXISTS `permissions` (
    `id` CHAR(36) NOT NULL,
    `code` VARCHAR(100) NOT NULL,
    `category` VARCHAR(100) NOT NULL,
    `description` VARCHAR(256) NULL,
    `is_deleted` TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_permissions_code` (`code`),
    KEY `ix_permissions_is_deleted` (`is_deleted`)
) ENGINE=InnoDB;

-- UserRoles Junction Table
CREATE TABLE IF NOT EXISTS `user_roles` (
    `user_id` CHAR(36) NOT NULL,
    `role_id` CHAR(36) NOT NULL,
    PRIMARY KEY (`user_id`, `role_id`),
    CONSTRAINT `fk_user_roles_users` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_user_roles_roles` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB;

-- RolePermissions Junction Table
CREATE TABLE IF NOT EXISTS `role_permissions` (
    `role_id` CHAR(36) NOT NULL,
    `permission_id` CHAR(36) NOT NULL,
    PRIMARY KEY (`role_id`, `permission_id`),
    CONSTRAINT `fk_role_permissions_roles` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_role_permissions_permissions` FOREIGN KEY (`permission_id`) REFERENCES `permissions` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB;

-- RefreshTokens Table
CREATE TABLE IF NOT EXISTS `refresh_tokens` (
    `id` CHAR(36) NOT NULL,
    `user_id` CHAR(36) NOT NULL,
    `token_hash` VARCHAR(500) NOT NULL,
    `expires_at_utc` DATETIME(6) NOT NULL,
    `created_by_ip` VARCHAR(45) NOT NULL,
    `created_at_utc` DATETIME(6) NOT NULL,
    `revoked_at_utc` DATETIME(6) NULL,
    `revoked_by_ip` VARCHAR(45) NULL,
    `replaced_by_token_hash` VARCHAR(500) NULL,
    `reason_revoked` VARCHAR(256) NULL,
    PRIMARY KEY (`id`),
    KEY `ix_refresh_tokens_user_id` (`user_id`),
    KEY `ix_refresh_tokens_token_hash` (`token_hash`),
    CONSTRAINT `fk_refresh_tokens_users` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB;

-- UserSessions Table
CREATE TABLE IF NOT EXISTS `user_sessions` (
    `id` CHAR(36) NOT NULL,
    `user_id` CHAR(36) NOT NULL,
    `device_name` VARCHAR(256) NULL,
    `ip_address` VARCHAR(45) NOT NULL,
    `user_agent` VARCHAR(500) NULL,
    `created_at_utc` DATETIME(6) NOT NULL,
    `last_activity_at_utc` DATETIME(6) NOT NULL,
    `is_revoked` TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`id`),
    KEY `ix_user_sessions_user_id` (`user_id`),
    CONSTRAINT `fk_user_sessions_users` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB;

-- MfaMethods Table
CREATE TABLE IF NOT EXISTS `mfa_methods` (
    `id` CHAR(36) NOT NULL,
    `user_id` CHAR(36) NOT NULL,
    `type` VARCHAR(50) NOT NULL,
    `secret` VARCHAR(500) NOT NULL,
    `is_enabled` TINYINT(1) NOT NULL DEFAULT 0,
    `created_at_utc` DATETIME(6) NOT NULL,
    PRIMARY KEY (`id`),
    KEY `ix_mfa_methods_user_id` (`user_id`),
    CONSTRAINT `fk_mfa_methods_users` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB;

-- SecurityEvents Table
CREATE TABLE IF NOT EXISTS `security_events` (
    `id` CHAR(36) NOT NULL,
    `user_id` CHAR(36) NULL,
    `event_type` VARCHAR(100) NOT NULL,
    `ip_address` VARCHAR(45) NOT NULL,
    `user_agent` VARCHAR(500) NULL,
    `details_json` LONGTEXT NULL,
    `created_at_utc` DATETIME(6) NOT NULL,
    PRIMARY KEY (`id`),
    KEY `ix_security_events_user_id` (`user_id`),
    KEY `ix_security_events_event_type` (`event_type`)
) ENGINE=InnoDB;

-- AuditLogs Table
CREATE TABLE IF NOT EXISTS `audit_logs` (
    `id` CHAR(36) NOT NULL,
    `user_id` CHAR(36) NULL,
    `action` VARCHAR(100) NOT NULL,
    `entity_name` VARCHAR(100) NOT NULL,
    `entity_id` VARCHAR(100) NULL,
    `old_values_json` LONGTEXT NULL,
    `new_values_json` LONGTEXT NULL,
    `ip_address` VARCHAR(45) NULL,
    `user_agent` VARCHAR(500) NULL,
    `created_at_utc` DATETIME(6) NOT NULL,
    PRIMARY KEY (`id`),
    KEY `ix_audit_logs_user_id` (`user_id`),
    KEY `ix_audit_logs_entity` (`entity_name`, `entity_id`)
) ENGINE=InnoDB;

-- =============================================================================
-- 2. Asynchronous Messaging & Reliability Infrastructure
-- =============================================================================

-- OutboxMessages Table
CREATE TABLE IF NOT EXISTS `outbox_messages` (
    `id` CHAR(36) NOT NULL,
    `type` VARCHAR(256) NOT NULL,
    `content` LONGTEXT NOT NULL,
    `occurred_on_utc` DATETIME(6) NOT NULL,
    `processed_on_utc` DATETIME(6) NULL,
    `error` LONGTEXT NULL,
    `retry_count` INT NOT NULL DEFAULT 0,
    PRIMARY KEY (`id`),
    KEY `ix_outbox_messages_unprocessed` (`processed_on_utc`, `occurred_on_utc`)
) ENGINE=InnoDB;

-- InboxMessages Table
CREATE TABLE IF NOT EXISTS `inbox_messages` (
    `id` CHAR(36) NOT NULL,
    `consumer_name` VARCHAR(256) NOT NULL,
    `type` VARCHAR(256) NOT NULL,
    `content` LONGTEXT NOT NULL,
    `received_on_utc` DATETIME(6) NOT NULL,
    `processed_on_utc` DATETIME(6) NULL,
    `error` LONGTEXT NULL,
    PRIMARY KEY (`id`, `consumer_name`),
    KEY `ix_inbox_messages_processed` (`processed_on_utc`)
) ENGINE=InnoDB;

-- IdempotencyRecords Table
CREATE TABLE IF NOT EXISTS `idempotency_records` (
    `id` CHAR(36) NOT NULL,
    `key` VARCHAR(256) NOT NULL,
    `operation_name` VARCHAR(100) NOT NULL,
    `request_hash` VARCHAR(128) NOT NULL,
    `response_json` LONGTEXT NULL,
    `status_code` INT NULL,
    `created_at_utc` DATETIME(6) NOT NULL,
    `expires_at_utc` DATETIME(6) NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_idempotency_records_key` (`key`),
    KEY `ix_idempotency_records_expires` (`expires_at_utc`)
) ENGINE=InnoDB;
