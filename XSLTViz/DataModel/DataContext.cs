namespace XSLTViz.DataModel
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class DataContext : DbContext
    {
        public DataContext()
            : base("name=XSLTViz")
        {
        }

        public DbSet<Project> Projects { get; set; }
    }
}