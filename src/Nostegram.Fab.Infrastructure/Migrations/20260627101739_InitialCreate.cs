using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nostegram.Fab.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Artists",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false, collation: "SQL_Latin1_General_CP1_CI_AS"),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FlipCardId = table.Column<int>(type: "int", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cards_Cards_FlipCardId",
                        column: x => x.FlipCardId,
                        principalSchema: "dbo",
                        principalTable: "Cards",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CardSubTypes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false, collation: "SQL_Latin1_General_CP1_CI_AS"),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardSubTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardTypes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false, collation: "SQL_Latin1_General_CP1_CI_AS"),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FabClasses",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false, collation: "SQL_Latin1_General_CP1_CI_AS"),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabClasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sets",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false, collation: "SQL_Latin1_General_CP1_CI_AS"),
                    SetCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false, collation: "SQL_Latin1_General_CP1_CI_AS"),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Talents",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false, collation: "SQL_Latin1_General_CP1_CI_AS"),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Talents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardVariants",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cost = table.Column<int>(type: "int", nullable: true),
                    Block = table.Column<int>(type: "int", nullable: true),
                    Attack = table.Column<int>(type: "int", nullable: true),
                    Health = table.Column<int>(type: "int", nullable: true),
                    Intellect = table.Column<int>(type: "int", nullable: true),
                    Pitch = table.Column<int>(type: "int", nullable: true),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardVariants_Cards_CardId",
                        column: x => x.CardId,
                        principalSchema: "dbo",
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardCardSubType",
                schema: "dbo",
                columns: table => new
                {
                    CardId = table.Column<int>(type: "int", nullable: false),
                    CardSubTypesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardCardSubType", x => new { x.CardId, x.CardSubTypesId });
                    table.ForeignKey(
                        name: "FK_CardCardSubType_CardSubTypes_CardSubTypesId",
                        column: x => x.CardSubTypesId,
                        principalSchema: "dbo",
                        principalTable: "CardSubTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CardCardSubType_Cards_CardId",
                        column: x => x.CardId,
                        principalSchema: "dbo",
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardCardType",
                schema: "dbo",
                columns: table => new
                {
                    CardId = table.Column<int>(type: "int", nullable: false),
                    CardTypesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardCardType", x => new { x.CardId, x.CardTypesId });
                    table.ForeignKey(
                        name: "FK_CardCardType_CardTypes_CardTypesId",
                        column: x => x.CardTypesId,
                        principalSchema: "dbo",
                        principalTable: "CardTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CardCardType_Cards_CardId",
                        column: x => x.CardId,
                        principalSchema: "dbo",
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardFabClass",
                schema: "dbo",
                columns: table => new
                {
                    CardId = table.Column<int>(type: "int", nullable: false),
                    FabClassesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardFabClass", x => new { x.CardId, x.FabClassesId });
                    table.ForeignKey(
                        name: "FK_CardFabClass_Cards_CardId",
                        column: x => x.CardId,
                        principalSchema: "dbo",
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardFabClass_FabClasses_FabClassesId",
                        column: x => x.FabClassesId,
                        principalSchema: "dbo",
                        principalTable: "FabClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CardTalent",
                schema: "dbo",
                columns: table => new
                {
                    CardId = table.Column<int>(type: "int", nullable: false),
                    TalentsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardTalent", x => new { x.CardId, x.TalentsId });
                    table.ForeignKey(
                        name: "FK_CardTalent_Cards_CardId",
                        column: x => x.CardId,
                        principalSchema: "dbo",
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardTalent_Talents_TalentsId",
                        column: x => x.TalentsId,
                        principalSchema: "dbo",
                        principalTable: "Talents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SetDetails",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rarity = table.Column<int>(type: "int", nullable: false),
                    SetId = table.Column<int>(type: "int", nullable: false),
                    CollectorNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArtistId = table.Column<int>(type: "int", nullable: false),
                    CardVariantId = table.Column<int>(type: "int", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetDetails_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalSchema: "dbo",
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetDetails_CardVariants_CardVariantId",
                        column: x => x.CardVariantId,
                        principalSchema: "dbo",
                        principalTable: "CardVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SetDetails_Sets_SetId",
                        column: x => x.SetId,
                        principalSchema: "dbo",
                        principalTable: "Sets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Artists_Name",
                schema: "dbo",
                table: "Artists",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardCardSubType_CardSubTypesId",
                schema: "dbo",
                table: "CardCardSubType",
                column: "CardSubTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_CardCardType_CardTypesId",
                schema: "dbo",
                table: "CardCardType",
                column: "CardTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_CardFabClass_FabClassesId",
                schema: "dbo",
                table: "CardFabClass",
                column: "FabClassesId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_FlipCardId",
                schema: "dbo",
                table: "Cards",
                column: "FlipCardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardSubTypes_Name",
                schema: "dbo",
                table: "CardSubTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardTalent_TalentsId",
                schema: "dbo",
                table: "CardTalent",
                column: "TalentsId");

            migrationBuilder.CreateIndex(
                name: "IX_CardTypes_Name",
                schema: "dbo",
                table: "CardTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardVariants_CardId",
                schema: "dbo",
                table: "CardVariants",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_FabClasses_Name",
                schema: "dbo",
                table: "FabClasses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetDetails_ArtistId",
                schema: "dbo",
                table: "SetDetails",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_SetDetails_CardVariantId",
                schema: "dbo",
                table: "SetDetails",
                column: "CardVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_SetDetails_SetId",
                schema: "dbo",
                table: "SetDetails",
                column: "SetId");

            migrationBuilder.CreateIndex(
                name: "IX_Sets_Name",
                schema: "dbo",
                table: "Sets",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sets_SetCode",
                schema: "dbo",
                table: "Sets",
                column: "SetCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Talents_Name",
                schema: "dbo",
                table: "Talents",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardCardSubType",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CardCardType",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CardFabClass",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CardTalent",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SetDetails",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CardSubTypes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CardTypes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "FabClasses",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Talents",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Artists",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CardVariants",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Sets",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Cards",
                schema: "dbo");
        }
    }
}
