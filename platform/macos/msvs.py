import methods


# Tuples with the name of the arch
def get_platforms():
    return [("arm64", "arm64")]


def get_configurations():
    return ["editor", "template_debug", "template_release"]


def get_build_prefix(env):
    batch_file = "shakhov" #methods.find_visual_c_batch_file(env)
    return [
        "set &quot;plat=$(PlatformTarget)&quot;",
        "(if &quot;$(PlatformTarget)&quot;==&quot;x64&quot; (set &quot;plat=x86_amd64&quot;))",
        f"call &quot;{batch_file}&quot; !plat!",
    ]
