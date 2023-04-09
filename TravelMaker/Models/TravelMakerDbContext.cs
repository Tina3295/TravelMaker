using System;
using System.Data.Entity;
using System.Linq;

namespace TravelMaker.Models
{
    public class TravelMakerDbContext : DbContext
    {
        // 您的內容已設定為使用應用程式組態檔 (App.config 或 Web.config)
        // 中的 'TravelMakerDbContext' 連接字串。根據預設，這個連接字串的目標是
        // 您的 LocalDb 執行個體上的 'TravelMaker.Models.TravelMakerDbContext' 資料庫。
        // 
        // 如果您的目標是其他資料庫和 (或) 提供者，請修改
        // 應用程式組態檔中的 'TravelMakerDbContext' 連接字串。
        public TravelMakerDbContext()
            : base("name=TravelMakerDbContext")
        {
        }

        // 針對您要包含在模型中的每種實體類型新增 DbSet。如需有關設定和使用
        // Code First 模型的詳細資訊，請參閱 http://go.microsoft.com/fwlink/?LinkId=390109。

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Attraction> Attractions { get; set; }
        public virtual DbSet<CategoryAttraction> CategoryAttractions { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<District> Districts { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<Tour> Tours { get; set; }
        public virtual DbSet<TourAttraction> TourAttractions { get; set; }
        public virtual DbSet<TourLike> TourLikes { get; set; }
        public virtual DbSet<RoomMember> RoomMembers { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<RoomAttraction> RoomAttractions { get; set; }
        public virtual DbSet<VoteDate> VoteDates { get; set; }
        public virtual DbSet<Vote> Votes { get; set; }
        public virtual DbSet<AttractionCollection> AttractionCollections { get; set; }
        public virtual DbSet<AttractionComment> AttractionComments { get; set; }
        public virtual DbSet<Blog> Blogs { get; set; }
        public virtual DbSet<BlogAttraction> BlogAttractions { get; set; }
        public virtual DbSet<BlogImage> BlogImages { get; set; }
        public virtual DbSet<BlogCollection> BlogCollections { get; set; }
        public virtual DbSet<BlogLike> BlogLikes { get; set; }
        public virtual DbSet<BlogComment> BlogComments { get; set; }
        public virtual DbSet<BlogReply> BlogReplies { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}