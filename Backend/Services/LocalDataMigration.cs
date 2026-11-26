namespace Control_de_Gastos.Services;

public static class LocalDataMigration
{
    public static void Prepare(string contentRoot, string? connection)
    {
        var target = Path.Combine(contentRoot, "App_Data");
        // Una conexión personalizada se respeta: no se traslada ninguna base automáticamente.
        if (connection != "Data Source=App_Data/react-demo.db") return;
        var repo = Path.GetFullPath(Path.Combine(contentRoot, ".."));
        if (!File.Exists(Path.Combine(repo, "ControlGastos.sln"))) return;
        var locations = new[] {
            target,
            Path.Combine(repo, "App_Data"),
            Path.Combine(repo, "server", "App_Data"),
            Path.Combine(repo, "React", "App_Data"),
            Path.Combine(repo, "React", "server", "App_Data")
        };
        var existing = locations.Where(p => File.Exists(Path.Combine(p, "react-demo.db"))).ToList();
        if (existing.Count > 1)
            throw new InvalidOperationException("Se encontró más de una react-demo.db. Conserva una copia de cada App_Data y selecciona la base correcta con ConnectionStrings__DemoConnection; no se sobrescribió nada.");
        if (existing.Count != 1 || existing[0] == target) return;
        if (Directory.Exists(target) && Directory.EnumerateFileSystemEntries(target).Any())
            throw new InvalidOperationException("Backend/App_Data ya contiene archivos. Respalda ambas carpetas y traslada la base y sus claves manualmente, sin sobrescribir.");
        // Respaldo y traslado de toda la carpeta, incluidos claves y auxiliares SQLite.
        var backup = Path.Combine(repo, "local-backups", "App_Data-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N")[..6]);
        CopyDirectory(existing[0], backup);
        if (Directory.Exists(target)) Directory.Delete(target);
        Directory.Move(existing[0], target);
        Console.WriteLine("Datos locales conservados en Backend/App_Data. Respaldo: " + backup);
    }
    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var file in Directory.EnumerateFiles(source)) File.Copy(file, Path.Combine(target, Path.GetFileName(file)), false);
        foreach (var directory in Directory.EnumerateDirectories(source)) CopyDirectory(directory, Path.Combine(target, Path.GetFileName(directory)));
    }
}
