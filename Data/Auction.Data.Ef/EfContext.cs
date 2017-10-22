using Auction.Domain;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Auction.Data.Ef
{
    public class EfContext : DbContext
    {
        public EfContext() : base("DefaultConnection")
        {
            Database.SetInitializer<EfContext>(null);
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Bid> Bids { get; set; }

        public DbSet<Market> Markets { get; set; }

        public DbSet<ProductMarket> ProductMarket {get; set;}

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Image> Images { get; set; }

        public List<string> Admins
        {
            get
            {
                return Database.SqlQuery<string>("select Name from dbo.ADMINS").ToList();
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //mapping

            modelBuilder.Entity<Product>().ToTable("PRODUCTS");
            modelBuilder.Entity<Product>().HasKey(x => x.Id);
            modelBuilder.Entity<Product>().Property(x => x.Id).HasColumnName("prd_id");
            modelBuilder.Entity<Product>().Property(x => x.Name).HasColumnName("prd_name");
            modelBuilder.Entity<Product>().Property(c => c.CategoryId).HasColumnName("prd_prdt_id");
            modelBuilder.Entity<Product>().Property(x => x.StartBid).HasColumnName("prd_start_bid");
            modelBuilder.Entity<Product>().Property(x => x.StartTime).HasColumnName("prd_start_time");
            modelBuilder.Entity<Product>().Property(x => x.EndTime).HasColumnName("prd_end_time");
            modelBuilder.Entity<Product>().Property(x => x.Duration).HasColumnName("prd_bid_dur");
            modelBuilder.Entity<Product>().Property(x => x.Description).HasColumnName("prd_desc"); 

            modelBuilder.Entity<Category>().ToTable("PRODUCT_TYPES");
            modelBuilder.Entity<Category>().HasKey(c => c.Id);
            modelBuilder.Entity<Category>().Property(c => c.Id).HasColumnName("prdt_id");
            modelBuilder.Entity<Category>().Property(c => c.Name).HasColumnName("prdt_name");

            modelBuilder.Entity<Bid>().ToTable("BIDS");
            modelBuilder.Entity<Bid>().HasKey(b => b.Id);
            modelBuilder.Entity<Bid>().Property(b => b.Id).HasColumnName("bid_id");
            modelBuilder.Entity<Bid>().Property(b => b.Sum).HasColumnName("bid_sum");
            modelBuilder.Entity<Bid>().Property(b => b.ProductId).HasColumnName("bid_prd_id");
            modelBuilder.Entity<Bid>().Property(b => b.BidTime).HasColumnName("bid_date");
            modelBuilder.Entity<Bid>().Property(b => b.User).HasColumnName("bid_user");
            modelBuilder.Entity<Bid>().Property(b => b.Winner).HasColumnName("bid_win");

            modelBuilder.Entity<Market>().ToTable("MARKETS");
            modelBuilder.Entity<Market>().HasKey(m => m.Id);
            modelBuilder.Entity<Market>().Property(m => m.Id).HasColumnName("mrkt_id");
            modelBuilder.Entity<Market>().Property(m => m.MarketName).HasColumnName("mrkt_name");
            modelBuilder.Entity<Market>().Property(m => m.MarketStart).HasColumnName("mrkt_start");
            modelBuilder.Entity<Market>().Property(m => m.MarketEnd).HasColumnName("mrkt_end");
            modelBuilder.Entity<Market>().Property(m => m.MarketStatus).HasColumnName("mrkt_status");
            modelBuilder.Entity<Market>().Property(m => m.MarketType).HasColumnName("mrkt_type_id");
            modelBuilder.Entity<Market>().Property(m => m.MarketClosed).HasColumnName("mrkt_closed");

            modelBuilder.Entity<ProductMarket>().ToTable("MRKT_PROD");
            modelBuilder.Entity<ProductMarket>().HasKey(i => i.Id);
            modelBuilder.Entity<ProductMarket>().Property(i => i.Id).HasColumnName("mrkt_prod_id");
            modelBuilder.Entity<ProductMarket>().Property(i => i.MarketId).HasColumnName("mrkt_id");
            modelBuilder.Entity<ProductMarket>().Property(i => i.ProductId).HasColumnName("prd_id");

            modelBuilder.Entity<Comment>().ToTable("COMMENTS");
            modelBuilder.Entity<Comment>().HasKey(j => j.Id);
            modelBuilder.Entity<Comment>().Property(j => j.Id).HasColumnName("com_id");
            modelBuilder.Entity<Comment>().Property(j => j.Text).HasColumnName("com_text");
            modelBuilder.Entity<Comment>().Property(j => j.User).HasColumnName("com_user");
            modelBuilder.Entity<Comment>().Property(j => j.Time).HasColumnName("com_time");
            modelBuilder.Entity<Comment>().Property(j => j.ProductId).HasColumnName("com_prd_id");

            modelBuilder.Entity<Image>().ToTable("IMAGES");
            modelBuilder.Entity<Image>().HasKey(i => i.Id);
            modelBuilder.Entity<Image>().Property(i => i.Id).HasColumnName("img_id");
            modelBuilder.Entity<Image>().Property(i => i.ProductId).HasColumnName("img_prd_id");
            modelBuilder.Entity<Image>().Property(i => i.Path).HasColumnName("img_file");
        }
    }
}
