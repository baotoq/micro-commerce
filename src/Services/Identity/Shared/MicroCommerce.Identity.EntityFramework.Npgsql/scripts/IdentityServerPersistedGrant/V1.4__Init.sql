CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "DeviceCodes" (
    "UserCode" character varying(200) NOT NULL,
    "DeviceCode" character varying(200) NOT NULL,
    "SubjectId" character varying(200) NULL,
    "ClientId" character varying(200) NOT NULL,
    "CreationTime" timestamp without time zone NOT NULL,
    "Expiration" timestamp without time zone NOT NULL,
    "Data" character varying(50000) NOT NULL,
    CONSTRAINT "PK_DeviceCodes" PRIMARY KEY ("UserCode")
);

CREATE TABLE "PersistedGrants" (
    "Key" character varying(200) NOT NULL,
    "Type" character varying(50) NOT NULL,
    "SubjectId" character varying(200) NULL,
    "ClientId" character varying(200) NOT NULL,
    "CreationTime" timestamp without time zone NOT NULL,
    "Expiration" timestamp without time zone NULL,
    "Data" character varying(50000) NOT NULL,
    CONSTRAINT "PK_PersistedGrants" PRIMARY KEY ("Key")
);

CREATE UNIQUE INDEX "IX_DeviceCodes_DeviceCode" ON "DeviceCodes" ("DeviceCode");

CREATE INDEX "IX_DeviceCodes_Expiration" ON "DeviceCodes" ("Expiration");

CREATE INDEX "IX_PersistedGrants_Expiration" ON "PersistedGrants" ("Expiration");

CREATE INDEX "IX_PersistedGrants_SubjectId_ClientId_Type" ON "PersistedGrants" ("SubjectId", "ClientId", "Type");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20191120100155_DbInit', '5.0.5');

COMMIT;

START TRANSACTION;

ALTER TABLE "PersistedGrants" ADD "ConsumedTime" timestamp without time zone NULL;

ALTER TABLE "PersistedGrants" ADD "Description" character varying(200) NULL;

ALTER TABLE "PersistedGrants" ADD "SessionId" character varying(100) NULL;

ALTER TABLE "DeviceCodes" ADD "Description" character varying(200) NULL;

ALTER TABLE "DeviceCodes" ADD "SessionId" character varying(100) NULL;

CREATE INDEX "IX_PersistedGrants_SubjectId_SessionId_Type" ON "PersistedGrants" ("SubjectId", "SessionId", "Type");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20201108165927_UpdateIdentityServerToVersion4', '5.0.5');

COMMIT;

