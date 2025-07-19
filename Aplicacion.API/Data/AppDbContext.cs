using Aplicacion.Modelos;
using Aplicacion.Modelos.Favorite;
using Aplicacion.Modelos.Identity;
using Aplicacion.Modelos.Implementations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
public class AppDbContext : IdentityDbContext<User, Role, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
    {
    }
    public DbSet<Aplicacion.Modelos.Musica> Musics { get; set; } = default!;

    public DbSet<Aplicacion.Modelos.Album> Albums { get; set; } = default!;

    public DbSet<Aplicacion.Modelos.Playlist> Playlists { get; set; } = default!;




    public DbSet<Aplicacion.Modelos.Suscription.SubscriptionPlan> SubscriptionPlans { get; set; } = default!;

    public DbSet<Aplicacion.Modelos.Suscription.UserSubscription> UserSubscriptions { get; set; } = default!;

    public DbSet<Aplicacion.Modelos.Implementations.Notificacion> Notifications { get; set; } = default!;

    public DbSet<Aplicacion.Modelos.Implementations.Download> Downloads { get; set; } = default!;

    public DbSet<Aplicacion.Modelos.Implementations.Follow> Follows { get; set; } = default!;


    // DbSets


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar FavoriteArtist
        modelBuilder.Entity<FavoritoArtista>()
            .HasOne(fa => fa.User)
            .WithMany(u => u.FavoriteArtists)
            .HasForeignKey(fa => fa.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<FavoritoArtista>()
            .HasOne(fa => fa.Artist)
            .WithMany() // Sin navegación inversa
            .HasForeignKey(fa => fa.ArtistId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configurar Follow (mismo problema)
        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Artist)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.ArtistId)
            .OnDelete(DeleteBehavior.NoAction);

    }
    public DbSet<Aplicacion.Modelos.PlaylistMusica> PlaylistMusics { get; set; } = default!;

    public DbSet<Aplicacion.Modelos.Favorite.FavoritoArtista> FavoriteArtists { get; set; } = default!;
    public DbSet<Aplicacion.Modelos.Favorite.FavoritoMusica> FavoriteMusics { get; set; } = default!;
}

