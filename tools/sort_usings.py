import glob, codecs, re


files = glob.glob("../src/Packages/**/*s.cs", recursive=True)
for file in files:
    print(file)
    usings = []
    aliases = []
    system_last = 0
    using_last = 0
    with codecs.open(file, "r+", "utf-8") as f:
        i = 0
        for line in f:
            clear_line = re.sub(r"[^a-zA-Z0-9.\r\n' '=;]+",'', line)
            i = i + 1
            if clear_line.startswith("using System"):
                system_last = i
                continue
            if "=" in clear_line:
                aliases.append(clear_line)
                continue
            usings.append(clear_line)
            if not clear_line.startswith("using"):
                using_last = i - 1
                break            
    usings = [w for w in usings if w != '\r\n']
    usings = sorted(usings, key=lambda w : re.sub(r"[^a-zA-Z0-9.' '=]+",'',w))
    usings = usings + aliases
    
    if system_last>0 and using_last > 0:
        with codecs.open(file, "r", "utf-8") as f:
            lines = f.readlines()

        lines[system_last : using_last] = usings

        with codecs.open(file, "w", "utf-8") as f:
            f.writelines(lines)