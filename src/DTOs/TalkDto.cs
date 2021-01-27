using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.DTOs
{
    public class TalkDto
    {
        public string Title { get; set; }
        public string Abstract { get; set; }
        public int Level { get; set; }

        public SpeakerDto Speaker { get; set; }
    }
}
