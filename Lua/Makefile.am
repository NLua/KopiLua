
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:TRACE;DEBUG;LUA_CORE;_WIN32;LUA_COMPAT_VARARG;LUA_COMPAT_MOD;LUA_COMPAT_GFIND;CATCH_EXCEPTIONS"
ASSEMBLY = ../Bin/Debug/Lua.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES = 
BUILD_DIR = ../Bin/Debug/

LUA_EXE_MDB_SOURCE=../Bin/Debug/Lua.exe.mdb
LUA_EXE_MDB=$(BUILD_DIR)/Lua.exe.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-define:TRACE;LUA_CORE;_WIN32;LUA_COMPAT_VARARG;LUA_COMPAT_MOD;LUA_COMPAT_GFIND;CATCH_EXCEPTIONS"
ASSEMBLY = ../Bin/Release/Lua.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = exe
PROJECT_REFERENCES = 
BUILD_DIR = ../Bin/Release/

LUA_EXE_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(LUA_EXE_MDB)  

BINARIES = \
	$(LUA)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(BINARIES) 

FILES = \
	lua.cs \
	Properties/AssemblyInfo.cs \
	../KopiLua/src/lapi.cs \
	../KopiLua/src/lauxlib.cs \
	../KopiLua/src/lbaselib.cs \
	../KopiLua/src/lcode.cs \
	../KopiLua/src/ldblib.cs \
	../KopiLua/src/ldebug.cs \
	../KopiLua/src/ldo.cs \
	../KopiLua/src/ldump.cs \
	../KopiLua/src/lfunc.cs \
	../KopiLua/src/lgc.cs \
	../KopiLua/src/linit.cs \
	../KopiLua/src/liolib.cs \
	../KopiLua/src/llex.cs \
	../KopiLua/src/llimits.cs \
	../KopiLua/src/lmathlib.cs \
	../KopiLua/src/lmem.cs \
	../KopiLua/src/loadlib.cs \
	../KopiLua/src/lobject.cs \
	../KopiLua/src/lopcodes.cs \
	../KopiLua/src/loslib.cs \
	../KopiLua/src/lparser.cs \
	../KopiLua/src/lstate.cs \
	../KopiLua/src/lstring.cs \
	../KopiLua/src/lstrlib.cs \
	../KopiLua/src/ltable.cs \
	../KopiLua/src/ltablib.cs \
	../KopiLua/src/ltm.cs \
	../KopiLua/src/lua.cs \
	../KopiLua/src/luaconf.cs \
	../KopiLua/src/lualib.cs \
	../KopiLua/src/lundump.cs \
	../KopiLua/src/lvm.cs \
	../KopiLua/src/lzio.cs \
	../KopiLua/src/print.cs \
	../KopiLua/src/printf/Tools.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	lua.in 

REFERENCES =  \
	Accessibility \
	System \
	System.Core \
	System.Data

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(BINARIES) 

include $(top_srcdir)/Makefile.include

LUA = $(BUILD_DIR)/lua

$(eval $(call emit-deploy-wrapper,LUA,lua,x))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
