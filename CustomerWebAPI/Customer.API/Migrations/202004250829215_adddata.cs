namespace Customer.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adddata : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Camp1", "Location1_Id", "dbo.Location1");
            DropForeignKey("dbo.Talk1", "Camp1_Id", "dbo.Camp1");
            DropForeignKey("dbo.Talk1", "Speaker1_Id", "dbo.Speaker1");
            DropIndex("dbo.Camp1", new[] { "Location1_Id" });
            DropIndex("dbo.Talk1", new[] { "Camp1_Id" });
            DropIndex("dbo.Talk1", new[] { "Speaker1_Id" });
            DropTable("dbo.Camp1");
            DropTable("dbo.Location1");
            DropTable("dbo.Speaker1");
            DropTable("dbo.Talk1");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Talk1",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Abstract = c.String(),
                        Level = c.Int(nullable: false),
                        Camp1_Id = c.Int(),
                        Speaker1_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Speaker1",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        MiddleName = c.String(),
                        Company = c.String(),
                        CompanyUrl = c.String(),
                        BlogUrl = c.String(),
                        Twitter = c.String(),
                        GitHub = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Location1",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VenueName = c.String(),
                        Address1 = c.String(),
                        Address2 = c.String(),
                        Address3 = c.String(),
                        CityTown = c.String(),
                        StateProvince = c.String(),
                        PostalCode = c.String(),
                        Country = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Camp1",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Moniker = c.String(),
                        EventDate = c.DateTime(nullable: false),
                        Length = c.Int(nullable: false),
                        Location1_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.Talk1", "Speaker1_Id");
            CreateIndex("dbo.Talk1", "Camp1_Id");
            CreateIndex("dbo.Camp1", "Location1_Id");
            AddForeignKey("dbo.Talk1", "Speaker1_Id", "dbo.Speaker1", "Id");
            AddForeignKey("dbo.Talk1", "Camp1_Id", "dbo.Camp1", "Id");
            AddForeignKey("dbo.Camp1", "Location1_Id", "dbo.Location1", "Id");
        }
    }
}
