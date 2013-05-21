
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:TRACE;DEBUG;LUA_CORE;_WIN32;LUA_COMPAT_VARARG;LUA_COMPAT_MOD;LUA_COMPAT_GFIND;CATCH_EXCEPTIONS"
ASSEMBLY = ../Bin/Debug/KopiLua.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ../Bin/Debug/

KOPILUA_DLL_MDB_SOURCE=../Bin/Debug/KopiLua.dll.mdb
KOPILUA_DLL_MDB=$(BUILD_DIR)/KopiLua.dll.mdb

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-define:TRACE;LUA_CORE;_WIN32;LUA_COMPAT_VARARG;LUA_COMPAT_MOD;LUA_COMPAT_GFIND;CATCH_EXCEPTIONS"
ASSEMBLY = ../Bin/Release/KopiLua.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = ../Bin/Release/

KOPILUA_DLL_MDB=

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(KOPILUA_DLL_MDB)  

LINUX_PKGCONFIG = \
	$(KOPILUA_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	Properties/AssemblyInfo_KopiLua.cs \
	src/lapi.cs \
	src/lauxlib.cs \
	src/lbaselib.cs \
	src/lcode.cs \
	src/ldblib.cs \
	src/ldebug.cs \
	src/ldo.cs \
	src/ldump.cs \
	src/lfunc.cs \
	src/lgc.cs \
	src/linit.cs \
	src/liolib.cs \
	src/llex.cs \
	src/llimits.cs \
	src/lmathlib.cs \
	src/lmem.cs \
	src/loadlib.cs \
	src/lobject.cs \
	src/lopcodes.cs \
	src/loslib.cs \
	src/lparser.cs \
	src/lstate.cs \
	src/lstring.cs \
	src/lstrlib.cs \
	src/ltable.cs \
	src/ltablib.cs \
	src/ltm.cs \
	src/lua.cs \
	src/luaconf.cs \
	src/lualib.cs \
	src/lundump.cs \
	src/lvm.cs \
	src/lzio.cs \
	src/print.cs \
	src/printf/Tools.cs \
	src/lnet.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	kopilua.pc.in 

REFERENCES =  \
	System \
	System.Core \
	System.Data.DataSetExtensions \
	System.Data

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

KOPILUA_PC = $(BUILD_DIR)/kopilua.pc

$(eval $(call emit-deploy-wrapper,KOPILUA_PC,kopilua.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
