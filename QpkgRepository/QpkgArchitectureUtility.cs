namespace ArxOne.Qnap;

public static class QpkgArchitectureUtility
{
    public static QpkgArchitecture? TryParse(string literal)
    {
        return literal.ToLower() switch
        {
            "x86" => QpkgArchitecture.X86,
            "x64" or "amd64" or "x86_64" => QpkgArchitecture.X86_64,
            "arm64" => QpkgArchitecture.Arm64,
            "arm" or "arm32" => QpkgArchitecture.Arm32,
            _ => null
        };
    }
}
