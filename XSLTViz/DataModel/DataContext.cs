namespace XSLTViz.DataModel
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class DataContext : DbContext
    {
        public DataContext()
            : base("name=DataContext")
        {
        }

        public DbSet<Project> Projects { get; set; }

        public DbSet<TemplateFile> Files { get; set; }

        public DbSet<Template> Templates { get; set; }
    }
}