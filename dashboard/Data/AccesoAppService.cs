using Microsoft.EntityFrameworkCore;

namespace dashboard.Data;

// Todas las consultas acá usan un DbContext propio (IDbContextFactory), independiente del
// DbContext scoped que usa Identity (UserManager/SignInManager) — ver Program.cs. Así, componentes
// que quedan montados todo el tiempo (TopbarNav/SidebarNav) no compiten por el mismo DbContext
// con la página que se está cargando.
public class AccesoAppService(IDbContextFactory<ApplicationDbContext> dbFactory)
{
    public record PerfilUsuario(string Nombre, string Correo, List<string> Roles);

    public async Task<PerfilUsuario?> ObtenerPerfilUsuarioAsync(string userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var user = await db.Users.FindAsync(userId);
        if (user is null) return null;

        var roles = await (from ur in db.UserRoles
                            join r in db.Roles on ur.RoleId equals r.Id
                            where ur.UserId == userId
                            select r.Name!).ToListAsync();

        return new PerfilUsuario(user.Nombre, user.Email ?? string.Empty, roles);
    }

    public async Task<Dictionary<string, List<string>>> ObtenerAccesoAsync(string userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var roleIds = await db.UserRoles.Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToListAsync();

        var apps = await db.PerfilesAplicacion.Where(p => roleIds.Contains(p.RoleId)).Select(p => p.App).Distinct().ToListAsync();
        var modulos = await db.PerfilesModulo.Where(p => roleIds.Contains(p.RoleId)).Select(p => new { p.App, p.Modulo }).Distinct().ToListAsync();

        return apps.ToDictionary(app => app, app => modulos.Where(m => m.App == app).Select(m => m.Modulo).OrderBy(m => PortalDefinicion.OrdenModulo(app, m)).ToList());
    }

    public record PerfilResumen(string RoleId, string Nombre, int CantidadUsuarios, List<string> Apps);

    public async Task<List<PerfilResumen>> ObtenerPerfilesAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var roles = await db.Roles.OrderBy(r => r.Name).ToListAsync();
        var conteos = await db.UserRoles.GroupBy(ur => ur.RoleId).Select(g => new { g.Key, Cantidad = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Cantidad);
        var apps = await db.PerfilesAplicacion.ToListAsync();

        return roles.Select(r => new PerfilResumen(
            r.Id,
            r.Name!,
            conteos.GetValueOrDefault(r.Id, 0),
            apps.Where(a => a.RoleId == r.Id).Select(a => a.App).OrderBy(a => a).ToList()
        )).ToList();
    }

    public async Task CrearPerfilAsync(string nombre)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var normalizado = nombre.ToUpperInvariant();
        if (await db.Roles.AnyAsync(r => r.NormalizedName == normalizado)) return;
        db.Roles.Add(new Microsoft.AspNetCore.Identity.IdentityRole(nombre) { NormalizedName = normalizado, ConcurrencyStamp = Guid.NewGuid().ToString() });
        await db.SaveChangesAsync();
    }

    public async Task<(List<Microsoft.AspNetCore.Identity.IdentityRole> Roles, HashSet<(string RoleId, string App)> Apps, HashSet<(string RoleId, string App, string Modulo)> Modulos)> ObtenerMatrizAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var roles = await db.Roles.OrderBy(r => r.Name).ToListAsync();
        var apps = (await db.PerfilesAplicacion.ToListAsync()).Select(a => (a.RoleId, a.App)).ToHashSet();
        var modulos = (await db.PerfilesModulo.ToListAsync()).Select(m => (m.RoleId, m.App, m.Modulo)).ToHashSet();
        return (roles, apps, modulos);
    }

    public async Task ToggleAppAsync(string roleId, string app, bool habilitado)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        if (habilitado)
        {
            if (!await db.PerfilesAplicacion.AnyAsync(p => p.RoleId == roleId && p.App == app))
                db.PerfilesAplicacion.Add(new Models.Admin.PerfilAplicacion { RoleId = roleId, App = app });
        }
        else
        {
            db.PerfilesAplicacion.RemoveRange(db.PerfilesAplicacion.Where(p => p.RoleId == roleId && p.App == app));
            db.PerfilesModulo.RemoveRange(db.PerfilesModulo.Where(p => p.RoleId == roleId && p.App == app));
        }
        await db.SaveChangesAsync();
    }

    public async Task ToggleModuloAsync(string roleId, string app, string modulo, bool habilitado)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        if (habilitado)
        {
            var appHabilitada = await db.PerfilesAplicacion.AnyAsync(p => p.RoleId == roleId && p.App == app);
            if (!appHabilitada) return;
            if (!await db.PerfilesModulo.AnyAsync(p => p.RoleId == roleId && p.App == app && p.Modulo == modulo))
                db.PerfilesModulo.Add(new Models.Admin.PerfilModulo { RoleId = roleId, App = app, Modulo = modulo });
        }
        else
        {
            db.PerfilesModulo.RemoveRange(db.PerfilesModulo.Where(p => p.RoleId == roleId && p.App == app && p.Modulo == modulo));
        }
        await db.SaveChangesAsync();
    }

    public record UsuarioResumen(string Id, string Nombre, string Correo, string? Perfil, bool Bloqueado);

    public async Task<List<UsuarioResumen>> ListarUsuariosAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var ahora = DateTimeOffset.UtcNow;

        var usuarios = await db.Users
            .Select(u => new
            {
                u.Id,
                u.Nombre,
                Correo = u.Email ?? "",
                Bloqueado = u.LockoutEnabled && u.LockoutEnd != null && u.LockoutEnd > ahora,
                Perfil = (from ur in db.UserRoles
                          join r in db.Roles on ur.RoleId equals r.Id
                          where ur.UserId == u.Id
                          select r.Name).FirstOrDefault(),
            })
            .OrderBy(u => u.Nombre)
            .ToListAsync();

        return usuarios.Select(u => new UsuarioResumen(u.Id, u.Nombre, u.Correo, u.Perfil, u.Bloqueado)).ToList();
    }
}
