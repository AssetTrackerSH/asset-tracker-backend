using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PortfolioTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Assets",
                columns: new[] { "Id", "Name", "Symbol", "Type" },
                values: new object[,]
                {
                    { 1, "Amerikan Doları", "USD", 0 },
                    { 2, "Euro", "EUR", 0 },
                    { 3, "İngiliz Sterlini", "GBP", 0 },
                    { 4, "İsviçre Frangı", "CHF", 0 },
                    { 5, "Japon Yeni", "JPY", 0 },
                    { 6, "Bitcoin", "BTC", 1 },
                    { 7, "Ethereum", "ETH", 1 },
                    { 8, "BNB", "BNB", 1 },
                    { 9, "Solana", "SOL", 1 },
                    { 10, "XRP", "XRP", 1 },
                    { 11, "Altın (gram)", "XAU", 3 },
                    { 12, "Gümüş (gram)", "XAG", 3 },
                    { 13, "Türk Hava Yolları", "THYAO", 2 },
                    { 14, "Garanti BBVA", "GARAN", 2 },
                    { 15, "Aselsan", "ASELS", 2 },
                    { 16, "Ereğli Demir Çelik", "EREGL", 2 },
                    { 17, "Şişe Cam", "SISE", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Assets",
                keyColumn: "Id",
                keyValue: 17);
        }
    }
}
