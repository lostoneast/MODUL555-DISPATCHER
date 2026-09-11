using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DispatcherApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Action = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OldValuesJson = table.Column<string>(type: "jsonb", nullable: true),
                    NewValuesJson = table.Column<string>(type: "jsonb", nullable: true),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "carriers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ContactInfo = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carriers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "construction_objects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Address = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_construction_objects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "plants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Address = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "product_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RollingStockType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    MinPayloadKg = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    MaxPayloadKg = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    LoadingPlatformCount = table.Column<int>(type: "integer", nullable: false),
                    LoadingPlatformLengthMm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    LoadingPlatformWidthMm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    AllowedRightLeftImbalanceKg = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    MaxCargoHeightMm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    CargoVolumeM3 = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    TotalTrainLengthMm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    TurningRadiusMm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "building_sections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConstructionObjectId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_building_sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_building_sections_construction_objects_ConstructionObjectId",
                        column: x => x.ConstructionObjectId,
                        principalTable: "construction_objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "construction_takts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConstructionObjectId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    PlannedProductionStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PlannedProductionEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_construction_takts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_construction_takts_construction_objects_ConstructionObjectId",
                        column: x => x.ConstructionObjectId,
                        principalTable: "construction_objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "unloading_points",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConstructionObjectId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unloading_points", x => x.Id);
                    table.ForeignKey(
                        name: "FK_unloading_points_construction_objects_ConstructionObjectId",
                        column: x => x.ConstructionObjectId,
                        principalTable: "construction_objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "production_lines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlantId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_production_lines_plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "storage_areas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlantId = table.Column<int>(type: "integer", nullable: true),
                    ConstructionObjectId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CapacityUnits = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storage_areas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_storage_areas_construction_objects_ConstructionObjectId",
                        column: x => x.ConstructionObjectId,
                        principalTable: "construction_objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_storage_areas_plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transport_routes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlantId = table.Column<int>(type: "integer", nullable: false),
                    ConstructionObjectId = table.Column<int>(type: "integer", nullable: false),
                    DistanceKm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    EstimatedTravelMinutes = table.Column<int>(type: "integer", nullable: true),
                    TurnoverCoefficientPerDay = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transport_routes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_transport_routes_construction_objects_ConstructionObjectId",
                        column: x => x.ConstructionObjectId,
                        principalTable: "construction_objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transport_routes_plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProductTypeId = table.Column<int>(type: "integer", nullable: false),
                    Mark = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    WidthMm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    HeightMm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    ThicknessMm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    CorniceWidthIncreaseMm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    TotalWidthWithCorniceMm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    ThicknessIncreaseMm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    RightBendMm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    LeftBendMm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    CladdingWidthWithBendsMm = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AdditionalInfo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_products_product_types_ProductTypeId",
                        column: x => x.ProductTypeId,
                        principalTable: "product_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleTypeId = table.Column<int>(type: "integer", nullable: false),
                    CarrierId = table.Column<int>(type: "integer", nullable: false),
                    Make = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Model = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicles_carriers_CarrierId",
                        column: x => x.CarrierId,
                        principalTable: "carriers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vehicles_vehicle_types_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "vehicle_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "floors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BuildingSectionId = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_floors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_floors_building_sections_BuildingSectionId",
                        column: x => x.BuildingSectionId,
                        principalTable: "building_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "line_capabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductionLineId = table.Column<int>(type: "integer", nullable: false),
                    ProductTypeId = table.Column<int>(type: "integer", nullable: false),
                    DefaultDailyCapacityUnits = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_line_capabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_line_capabilities_product_types_ProductTypeId",
                        column: x => x.ProductTypeId,
                        principalTable: "product_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_line_capabilities_production_lines_ProductionLineId",
                        column: x => x.ProductionLineId,
                        principalTable: "production_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_identifiers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_identifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_identifiers_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "storage_placements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    StorageAreaId = table.Column<int>(type: "integer", nullable: false),
                    ArrivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DepartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storage_placements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_storage_placements_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_storage_placements_storage_areas_StorageAreaId",
                        column: x => x.StorageAreaId,
                        principalTable: "storage_areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "takt_assignments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConstructionTaktId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ValidFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_takt_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_takt_assignments_construction_takts_ConstructionTaktId",
                        column: x => x.ConstructionTaktId,
                        principalTable: "construction_takts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_takt_assignments_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trips",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TripNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PlantId = table.Column<int>(type: "integer", nullable: false),
                    ConstructionObjectId = table.Column<int>(type: "integer", nullable: false),
                    UnloadingPointId = table.Column<int>(type: "integer", nullable: true),
                    VehicleId = table.Column<long>(type: "bigint", nullable: false),
                    PlannedLoadingAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PlannedDepartureAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PlannedArrivalAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualLoadingAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualDepartureAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActualArrivalAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trips_construction_objects_ConstructionObjectId",
                        column: x => x.ConstructionObjectId,
                        principalTable: "construction_objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trips_plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trips_unloading_points_UnloadingPointId",
                        column: x => x.UnloadingPointId,
                        principalTable: "unloading_points",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trips_vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_assignments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ConstructionObjectId = table.Column<int>(type: "integer", nullable: false),
                    BuildingSectionId = table.Column<int>(type: "integer", nullable: true),
                    FloorId = table.Column<int>(type: "integer", nullable: true),
                    InstallationNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ValidFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_assignments_building_sections_BuildingSectionId",
                        column: x => x.BuildingSectionId,
                        principalTable: "building_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_assignments_construction_objects_ConstructionObject~",
                        column: x => x.ConstructionObjectId,
                        principalTable: "construction_objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_assignments_floors_FloorId",
                        column: x => x.FloorId,
                        principalTable: "floors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_assignments_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "project_positions",
                columns: table => new
                {
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ConstructionObjectId = table.Column<int>(type: "integer", nullable: false),
                    BuildingSectionId = table.Column<int>(type: "integer", nullable: true),
                    FloorId = table.Column<int>(type: "integer", nullable: true),
                    InstallationNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_positions", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_project_positions_building_sections_BuildingSectionId",
                        column: x => x.BuildingSectionId,
                        principalTable: "building_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_project_positions_construction_objects_ConstructionObjectId",
                        column: x => x.ConstructionObjectId,
                        principalTable: "construction_objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_project_positions_floors_FloorId",
                        column: x => x.FloorId,
                        principalTable: "floors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_project_positions_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "capacity_overrides",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LineCapabilityId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    CapacityUnits = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capacity_overrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_capacity_overrides_line_capabilities_LineCapabilityId",
                        column: x => x.LineCapabilityId,
                        principalTable: "line_capabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trip_items",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TripId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    LoadingSequence = table.Column<int>(type: "integer", nullable: false),
                    AddedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AddedByUserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trip_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trip_items_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trip_items_trips_TripId",
                        column: x => x.TripId,
                        principalTable: "trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "demand_revisions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DemandId = table.Column<long>(type: "bigint", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    ConstructionTaktId = table.Column<long>(type: "bigint", nullable: false),
                    RequiredProductionStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RequiredProductionEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EarliestDeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RequiredDeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PriorityLevel = table.Column<int>(type: "integer", nullable: false),
                    PriorityOrder = table.Column<int>(type: "integer", nullable: false),
                    ChangeReason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demand_revisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_demand_revisions_construction_takts_ConstructionTaktId",
                        column: x => x.ConstructionTaktId,
                        principalTable: "construction_takts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "demands",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrentRevisionId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_demands_demand_revisions_CurrentRevisionId",
                        column: x => x.CurrentRevisionId,
                        principalTable: "demand_revisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_demands_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "production_assignments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DemandRevisionId = table.Column<long>(type: "bigint", nullable: false),
                    ProductionLineId = table.Column<int>(type: "integer", nullable: false),
                    PlannedProductionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AssignmentMethod = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: true),
                    DecisionSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    OverrideReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ValidFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_production_assignments_demand_revisions_DemandRevisionId",
                        column: x => x.DemandRevisionId,
                        principalTable: "demand_revisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_production_assignments_production_lines_ProductionLineId",
                        column: x => x.ProductionLineId,
                        principalTable: "production_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_EntityType_EntityId",
                table: "audit_events",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_Timestamp",
                table: "audit_events",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_building_sections_ConstructionObjectId",
                table: "building_sections",
                column: "ConstructionObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_capacity_overrides_LineCapabilityId_Date",
                table: "capacity_overrides",
                columns: new[] { "LineCapabilityId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_construction_objects_Code",
                table: "construction_objects",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_construction_takts_ConstructionObjectId",
                table: "construction_takts",
                column: "ConstructionObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_demand_revisions_ConstructionTaktId",
                table: "demand_revisions",
                column: "ConstructionTaktId");

            migrationBuilder.CreateIndex(
                name: "IX_demand_revisions_DemandId_RevisionNumber",
                table: "demand_revisions",
                columns: new[] { "DemandId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_demands_CurrentRevisionId",
                table: "demands",
                column: "CurrentRevisionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_demands_ProductId",
                table: "demands",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_floors_BuildingSectionId",
                table: "floors",
                column: "BuildingSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_line_capabilities_ProductionLineId_ProductTypeId",
                table: "line_capabilities",
                columns: new[] { "ProductionLineId", "ProductTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_line_capabilities_ProductTypeId",
                table: "line_capabilities",
                column: "ProductTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_plants_Code",
                table: "plants",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_assignments_BuildingSectionId",
                table: "product_assignments",
                column: "BuildingSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_product_assignments_ConstructionObjectId",
                table: "product_assignments",
                column: "ConstructionObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_product_assignments_FloorId",
                table: "product_assignments",
                column: "FloorId");

            migrationBuilder.CreateIndex(
                name: "IX_product_assignments_ProductId",
                table: "product_assignments",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_product_identifiers_ProductId",
                table: "product_identifiers",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_product_identifiers_Type_Value",
                table: "product_identifiers",
                columns: new[] { "Type", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_types_Code",
                table: "product_types",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_production_assignments_DemandRevisionId",
                table: "production_assignments",
                column: "DemandRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_production_assignments_ProductionLineId",
                table: "production_assignments",
                column: "ProductionLineId");

            migrationBuilder.CreateIndex(
                name: "IX_production_lines_PlantId",
                table: "production_lines",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_products_ProductCode",
                table: "products",
                column: "ProductCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_ProductTypeId",
                table: "products",
                column: "ProductTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_project_positions_BuildingSectionId",
                table: "project_positions",
                column: "BuildingSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_project_positions_ConstructionObjectId",
                table: "project_positions",
                column: "ConstructionObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_project_positions_FloorId",
                table: "project_positions",
                column: "FloorId");

            migrationBuilder.CreateIndex(
                name: "IX_storage_areas_ConstructionObjectId",
                table: "storage_areas",
                column: "ConstructionObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_storage_areas_PlantId",
                table: "storage_areas",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_storage_placements_ProductId",
                table: "storage_placements",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_storage_placements_StorageAreaId",
                table: "storage_placements",
                column: "StorageAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_takt_assignments_ConstructionTaktId",
                table: "takt_assignments",
                column: "ConstructionTaktId");

            migrationBuilder.CreateIndex(
                name: "IX_takt_assignments_ProductId",
                table: "takt_assignments",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_transport_routes_ConstructionObjectId",
                table: "transport_routes",
                column: "ConstructionObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_transport_routes_PlantId_ConstructionObjectId",
                table: "transport_routes",
                columns: new[] { "PlantId", "ConstructionObjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trip_items_ProductId",
                table: "trip_items",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_trip_items_TripId_ProductId",
                table: "trip_items",
                columns: new[] { "TripId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trips_ConstructionObjectId",
                table: "trips",
                column: "ConstructionObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_trips_PlantId",
                table: "trips",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_trips_TripNumber",
                table: "trips",
                column: "TripNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trips_UnloadingPointId",
                table: "trips",
                column: "UnloadingPointId");

            migrationBuilder.CreateIndex(
                name: "IX_trips_VehicleId",
                table: "trips",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_unloading_points_ConstructionObjectId",
                table: "unloading_points",
                column: "ConstructionObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_CarrierId",
                table: "vehicles",
                column: "CarrierId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_RegistrationNumber",
                table: "vehicles",
                column: "RegistrationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_VehicleTypeId",
                table: "vehicles",
                column: "VehicleTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_demand_revisions_demands_DemandId",
                table: "demand_revisions",
                column: "DemandId",
                principalTable: "demands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_construction_takts_construction_objects_ConstructionObjectId",
                table: "construction_takts");

            migrationBuilder.DropForeignKey(
                name: "FK_demand_revisions_construction_takts_ConstructionTaktId",
                table: "demand_revisions");

            migrationBuilder.DropForeignKey(
                name: "FK_demand_revisions_demands_DemandId",
                table: "demand_revisions");

            migrationBuilder.DropTable(
                name: "audit_events");

            migrationBuilder.DropTable(
                name: "capacity_overrides");

            migrationBuilder.DropTable(
                name: "product_assignments");

            migrationBuilder.DropTable(
                name: "product_identifiers");

            migrationBuilder.DropTable(
                name: "production_assignments");

            migrationBuilder.DropTable(
                name: "project_positions");

            migrationBuilder.DropTable(
                name: "storage_placements");

            migrationBuilder.DropTable(
                name: "takt_assignments");

            migrationBuilder.DropTable(
                name: "transport_routes");

            migrationBuilder.DropTable(
                name: "trip_items");

            migrationBuilder.DropTable(
                name: "line_capabilities");

            migrationBuilder.DropTable(
                name: "floors");

            migrationBuilder.DropTable(
                name: "storage_areas");

            migrationBuilder.DropTable(
                name: "trips");

            migrationBuilder.DropTable(
                name: "production_lines");

            migrationBuilder.DropTable(
                name: "building_sections");

            migrationBuilder.DropTable(
                name: "unloading_points");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "plants");

            migrationBuilder.DropTable(
                name: "carriers");

            migrationBuilder.DropTable(
                name: "vehicle_types");

            migrationBuilder.DropTable(
                name: "construction_objects");

            migrationBuilder.DropTable(
                name: "construction_takts");

            migrationBuilder.DropTable(
                name: "demands");

            migrationBuilder.DropTable(
                name: "demand_revisions");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "product_types");
        }
    }
}
