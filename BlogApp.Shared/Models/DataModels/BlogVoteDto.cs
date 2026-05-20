using BlogApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp.Models.DataModels
{
    public class BlogVoteDto
    {
        public long BlogId { get; set; } // to Blog
        public long AppUserId { get; set; } // from User
        public VoteType VoteType { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"<(BlogId={BlogId}), (AppUserId={AppUserId}), (VoteType={VoteType.ToString()}), (ReturnUrl={ReturnUrl})>";
        }
    }
}
