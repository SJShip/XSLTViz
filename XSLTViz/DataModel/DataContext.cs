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

        public DbSet<File> Files { get; set; }

        public DbSet<Template> Templates { get; set; }

        public DbSet<FilesRelation> FilesRelations { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}