﻿using Utility.Entities;


namespace Movies.Entities
{
    public class ActedIn : IRelation
    {
        public string FromId { get; set; }
        public string ToId { get; set; }
        public int Order { get; set; }
        public string Role { get; set; }
    }

    public class Directed : IRelation
    {
        public string FromId { get; set; }
        public string ToId { get; set; }
    }

    public class Wrote : IRelation
    {
        public string FromId { get; set; }
        public string ToId { get; set; }
    }

    public class Produced : IRelation
    {
        public string FromId { get; set; }
        public string ToId { get; set; }
    }
}