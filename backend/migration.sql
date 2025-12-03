CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118043543_InitialCreate') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "FirstName" character varying(100) NOT NULL,
        "LastName" character varying(100) NOT NULL,
        "Email" character varying(255) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118043543_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251118043543_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251118043543_InitialCreate', '9.0.1');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124094030_AddReceiptOcrModels') THEN
    CREATE TABLE "Receipts" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "ImagePath" character varying(500) NOT NULL,
        "OriginalFileName" character varying(255) NOT NULL,
        "Status" integer NOT NULL,
        "MerchantName" character varying(255),
        "ReceiptDate" timestamp with time zone,
        "TotalAmount" numeric(18,2),
        "RawOcrText" text,
        "OcrConfidence" numeric(5,2),
        "ErrorMessage" character varying(1000),
        "UploadedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Receipts" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Receipts_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124094030_AddReceiptOcrModels') THEN
    CREATE TABLE "ReceiptItems" (
        "Id" uuid NOT NULL,
        "ReceiptId" uuid NOT NULL,
        "ItemName" character varying(500) NOT NULL,
        "Quantity" integer NOT NULL,
        "UnitPrice" numeric(18,2),
        "TotalPrice" numeric(18,2) NOT NULL,
        "LineNumber" integer NOT NULL,
        "IsManuallyAdded" boolean NOT NULL,
        "OcrConfidence" numeric(5,2),
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_ReceiptItems" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ReceiptItems_Receipts_ReceiptId" FOREIGN KEY ("ReceiptId") REFERENCES "Receipts" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124094030_AddReceiptOcrModels') THEN
    CREATE INDEX "IX_ReceiptItems_LineNumber" ON "ReceiptItems" ("LineNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124094030_AddReceiptOcrModels') THEN
    CREATE INDEX "IX_ReceiptItems_ReceiptId" ON "ReceiptItems" ("ReceiptId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124094030_AddReceiptOcrModels') THEN
    CREATE INDEX "IX_Receipts_Status" ON "Receipts" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124094030_AddReceiptOcrModels') THEN
    CREATE INDEX "IX_Receipts_UploadedAt" ON "Receipts" ("UploadedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124094030_AddReceiptOcrModels') THEN
    CREATE INDEX "IX_Receipts_UserId" ON "Receipts" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124094030_AddReceiptOcrModels') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251124094030_AddReceiptOcrModels', '9.0.1');
    END IF;
END $EF$;
COMMIT;

