-- PATCH Pending Migrations (Idempotent)

-- Helper function idea (ไม่จำเป็น) : ข้ามไป ใช้ DO แยก

-- 20250722025415_RenameColumnTotolWorkTime
DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250722025415_RenameColumnTotolWorkTime') THEN
        -- Guess: rename TotolWorkTime -> TotalWorkTime (เช็คใน WorkTime)
        -- IF EXISTS(...) THEN ALTER TABLE ... RENAME COLUMN ... END IF;
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250722025415_RenameColumnTotolWorkTime','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250722064529_AddStockCate') THEN
        -- TODO: ถ้ายังไม่มีตาราง StockCategory หรือคอลัมน์ StockCategoryId ให้เพิ่ม
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250722064529_AddStockCate','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250722072210_AddFKStock') THEN
        -- TODO: เพิ่ม FK ที่เกี่ยวกับ StockCategory (ถ้ายังขาด)
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250722072210_AddFKStock','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250722072703_AddFKStockToContext') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250722072703_AddFKStockToContext','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250723061820_AddRecentStockLogFix') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250723061820_AddRecentStockLogFix','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250723075726_AddDescriptionStocklogtype') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250723075726_AddDescriptionStocklogtype','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250723094752_FixStructureStockLog') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250723094752_FixStructureStockLog','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250724152343_ClearMigrat') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250724152343_ClearMigrat','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250725073708_AddUserpermissionMore2') THEN
        -- TODO: ตรวจตาราง/คอล permission
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250725073708_AddUserpermissionMore2','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250725085945_UpdateResent') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250725085945_UpdateResent','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250728041912_CostStatus') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250728041912_CostStatus','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250729045908_UpdateCost') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250729045908_UpdateCost','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250729074943_UpdateCostAndStockTable') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250729074943_UpdateCostAndStockTable','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250729084715_AddCostIDtoStockLog') THEN
        -- ตรวจว่า StockLog มี CostId หรือยัง; ถ้าไม่มีและต้องการจริง: ALTER TABLE "StockLog" ADD "CostId" int NULL;
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250729084715_AddCostIDtoStockLog','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250729094023_RemoveSupplierSupplyID') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250729094023_RemoveSupplierSupplyID','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250729094308_RemoveSupplierSupplyID2') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250729094308_RemoveSupplierSupplyID2','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250729155151_GrabPrice') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250729155151_GrabPrice','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250729155839_GrabPrice2') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250729155839_GrabPrice2','9.0.6');
    END IF;
END $$;

-- NOTE: 20250812043838_renameIsPurchase / 20250812045507_renamePurchase / 20250815041457_AddCreateBy... / 20250818065546_RenameIsPurchaseStockLog ถูกลงแล้ว

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250815043624_RemoveRoleFromUser') THEN
        -- ถ้าต้องการจำลองจริง:
        -- IF EXISTS(SELECT 1 FROM information_schema.columns WHERE table_name='Users' AND column_name='Role') THEN
        --     ALTER TABLE "Users" DROP COLUMN "Role";
        -- END IF;
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250815043624_RemoveRoleFromUser','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250815063136_CostAllowNull') THEN
        -- ตัวนี้น่าจะเปลี่ยนบาง column ของ Cost เป็น NULLABLE; ตรวจแล้วคอลัมน์อนุญาต NULL อยู่หรือไม่
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250815063136_CostAllowNull','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250818082111_SyncSnapshot') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250818082111_SyncSnapshot','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250818090827_SetupdatedateStocklog') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250818090827_SetupdatedateStocklog','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250818091540_SetupdatedateAllowNull') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250818091540_SetupdatedateAllowNull','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250819023228_AddStockCountDatetimeToStockLog') THEN
        -- ถ้าขาดคอลัมน์ CountDateTime (เดา) ให้เพิ่ม
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250819023228_AddStockCountDatetimeToStockLog','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250819024725_AddStockFKToStockLog') THEN
        -- ตรวจ FK ที่เกี่ยวข้อง
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250819024725_AddStockFKToStockLog','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250821105757_AddStockUnitCostHistory') THEN
        -- อาจมีตาราง StockUnitCostHistory
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250821105757_AddStockUnitCostHistory','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250821110128_AddStockUnitCostHistory2') THEN
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250821110128_AddStockUnitCostHistory2','9.0.6');
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId"='20250825031858_AddUserSite') THEN
        -- Add Site column (real DDL)
        IF NOT EXISTS(
            SELECT 1 FROM information_schema.columns
            WHERE table_name='Users' AND column_name='Site'
        ) THEN
            ALTER TABLE "Users" ADD "Site" text NULL;
        END IF;
        INSERT INTO "__EFMigrationsHistory" VALUES ('20250825031858_AddUserSite','9.0.6');
    END IF;
END $$;

-- END PATCH