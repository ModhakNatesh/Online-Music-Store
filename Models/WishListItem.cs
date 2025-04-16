using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineMusicStore.Models
{
    public class WishListItem
    {
        [Key]
        public int WishListItemId { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int MusicItemId { get; set; }

        [ForeignKey("MusicItemId")]
        public virtual MusicItem MusicItem { get; set; }
    }
}
