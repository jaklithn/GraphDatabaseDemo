﻿using System;


namespace Movies.Entities
{
    public class Movie
    {
        public int TmdbId { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public string CollectionInfo { get; set; }
        public string[] Genres { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Plot { get; set; }
        public string Tagline { get; set; }
        public string OriginalLanguage { get; set; }
        public double Rating { get; set; }
        public int Budget { get; set; }
    }
}