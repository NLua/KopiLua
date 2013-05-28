/*
** $Id: loslib.c,v 1.19.1.3 2008/01/18 16:38:18 roberto Exp $
** Standard Operating System library
** See Copyright Notice in lua.h
*/

using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace KopiLua
{
	using TValue = Lua.lua_TValue;
	using StkId = Lua.lua_TValue;
	using lua_Integer = System.Int32;
	using lua_Number = System.Double;

	public partial class Lua
	{
		private static int os_pushresult (lua_State L, int i, CharPtr filename) {
		  int en = errno();  /* calls to Lua API may change this value */
		  if (i != 0) {
			lua_pushboolean(L, 1);
			return 1;
		  }
		  else {
			lua_pushnil(L);
			lua_pushfstring(L, "%s: %s", filename, strerror(en));
			lua_pushinteger(L, en);
			return 3;
		  }
		}


		private static int os_execute (lua_State L) {
#if XBOX || SILVERLIGHT
			luaL_error(L, "os_execute not supported on XBox360");
#else
			CharPtr param = luaL_optstring(L, 1, null);
			if (param == null) {
				lua_pushinteger (L, 1);
				return 1;
			}
			CharPtr strCmdLine = "/C regenresx " + luaL_optstring(L, 1, null);
			System.Diagnostics.Process proc = new System.Diagnostics.Process();
			proc.EnableRaisingEvents=false;
			proc.StartInfo.FileName = "CMD.exe";
			proc.StartInfo.Arguments = strCmdLine.ToString();
			proc.Start();
			proc.WaitForExit();
			lua_pushinteger(L, proc.ExitCode);
#endif
			return 1;
		}


		private static int os_remove (lua_State L) {
		  CharPtr filename = luaL_checkstring(L, 1);
		  int result = 1;
		  try {File.Delete(filename.ToString());} catch {result = 0;}
		  return os_pushresult(L, result, filename);
		}


		private static int os_rename (lua_State L) {
			CharPtr fromname = luaL_checkstring(L, 1);
		  CharPtr toname = luaL_checkstring(L, 2);
		  int result;
		  try
		  {
			  File.Move(fromname.ToString(), toname.ToString());
			  result = 0;
		  }
		  catch
		  {
			  result = 1; // todo: this should be a proper error code
		  }
		  return os_pushresult(L, result, fromname);
		}


		private static int os_tmpname (lua_State L) {
#if XBOX
		  luaL_error(L, "os_tmpname not supported on Xbox360");
#else
		  lua_pushstring(L, Path.GetTempFileName());
#endif
		  return 1;
		}


		private static int os_getenv (lua_State L) {
		  lua_pushstring(L, getenv(luaL_checkstring(L, 1)));  /* if null push nil */
		  return 1;
		}


		private static int os_clock (lua_State L) {
		  long ticks = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		  lua_pushnumber(L, ((lua_Number)ticks)/(lua_Number)1000);
		  return 1;
		}


		/*
		** {======================================================
		** Time/Date operations
		** { year=%Y, month=%m, day=%d, hour=%H, min=%M, sec=%S,
		**   wday=%w+1, yday=%j, isdst=? }
		** =======================================================
		*/

		private static void setfield (lua_State L, CharPtr key, int value) {
		  lua_pushinteger(L, value);
		  lua_setfield(L, -2, key);
		}

		private static void setboolfield (lua_State L, CharPtr key, int value) {
		  if (value < 0)  /* undefined? */
			return;  /* does not set field */
		  lua_pushboolean(L, value);
		  lua_setfield(L, -2, key);
		}

		private static int getboolfield (lua_State L, CharPtr key) {
		  int res;
		  lua_getfield(L, -1, key);
		  res = lua_isnil(L, -1) ? -1 : lua_toboolean(L, -1);
		  lua_pop(L, 1);
		  return res;
		}

		private static int getfield (lua_State L, CharPtr key, int d) {
		  int res;
		  lua_getfield(L, -1, key);
		  if (lua_isnumber(L, -1) != 0)
			res = (int)lua_tointeger(L, -1);
		  else {
			if (d < 0)
			  return luaL_error(L, "field " + LUA_QS + " missing in date table", key);
			res = d;
		  }
		  lua_pop(L, 1);
		  return res;
		}


		private static int os_date (lua_State L) {
		  CharPtr s = luaL_optstring(L, 1, "%c");
		  DateTime stm;
		  if (s[0] == '!') {  /* UTC? */
			stm = DateTime.UtcNow;
			s.inc();  /* skip `!' */
		  }
		  else
			  stm = DateTime.Now;
		  if (strcmp(s, "*t") == 0) {
			lua_createtable(L, 0, 9);  /* 9 = number of fields */
			setfield(L, "sec", stm.Second);
			setfield(L, "min", stm.Minute);
			setfield(L, "hour", stm.Hour);
			setfield(L, "day", stm.Day);
			setfield(L, "month", stm.Month);
			setfield(L, "year", stm.Year);
			setfield(L, "wday", (int)stm.DayOfWeek);
			setfield(L, "yday", stm.DayOfYear);
			setboolfield(L, "isdst", stm.IsDaylightSavingTime() ? 1 : 0);
		  }
		  else {
			CharPtr cc = new char[3];
			luaL_Buffer b = new luaL_Buffer();
			cc[0] = '%'; cc[2] = '\0';
			luaL_buffinit(L, b);
			for (; s[0] != 0; s.inc()) {
			  if (s[0] != '%' || s[1] == '\0')  /* no conversion specifier? */
			    luaL_addchar(b, s[0]);
			  else {
			    uint reslen;
			    CharPtr buff = new char[200];  /* should be big enough for any conversion result */
			    s.inc();
			    cc[1] = s[0];
			    reslen = strftime(buff, (uint)buff.chars.Length, cc, stm);
			    buff.index = 0;
			    luaL_addlstring(b, buff, reslen);
			  }
			}
			luaL_pushresult(b);
		  }
			return 1;
		}

		#region strftime c# implementation
		
		// This strftime implementation has been made following the
		// Sanos OS open-source strftime.c implementation at
		// http://www.jbox.dk/sanos/source/lib/strftime.c.html
		
		private static uint strftime(CharPtr s, uint maxsize, CharPtr format, DateTime t)
		{
			int sIndex = s.index;

			CharPtr p = strftime_fmt((format as object) == null ? "%c" : format, t, s, s.add((int)maxsize));
			if (p == s + maxsize) return 0;
			p[0] = '\0';

			return (uint)Math.Abs(s.index - sIndex);
		}

		private static CharPtr strftime_fmt(CharPtr baseFormat, DateTime t, CharPtr pt, CharPtr ptlim)
		{
			CharPtr format = new CharPtr(baseFormat);

			for (; format[0] != 0; format.inc())
			{

				if (format == '%')
				{

					format.inc();

					if (format == 'E')
					{
						format.inc(); // Alternate Era is ignored
					}
					else if (format == 'O')
					{
						format.inc(); // Alternate numeric symbols is ignored
					}

					switch (format[0])
					{
						case '\0':
							format.dec();
							break;

						case 'A': // Full day of week
							//pt = _add((t->tm_wday < 0 || t->tm_wday > 6) ? "?" : _days[t->tm_wday], pt, ptlim);
							pt = strftime_add(t.ToString("dddd"), pt, ptlim);
							continue;

						case 'a': // Abbreviated day of week
							//pt = _add((t->tm_wday < 0 || t->tm_wday > 6) ? "?" : _days_abbrev[t->tm_wday], pt, ptlim);
							pt = strftime_add(t.ToString("ddd"), pt, ptlim);
							continue;

						case 'B': // Full month name
							//pt = _add((t->tm_mon < 0 || t->tm_mon > 11) ? "?" : _months[t->tm_mon], pt, ptlim);
							pt = strftime_add(t.ToString("MMMM"), pt, ptlim);
							continue;

						case 'b': // Abbreviated month name
						case 'h': // Abbreviated month name
							//pt = _add((t->tm_mon < 0 || t->tm_mon > 11) ? "?" : _months_abbrev[t->tm_mon], pt, ptlim);
							pt = strftime_add(t.ToString("MMM"), pt, ptlim);
							continue;

						case 'C': // First two digits of year (a.k.a. Year divided by 100 and truncated to integer (00-99))
							//pt = _conv((t->tm_year + TM_YEAR_BASE) / 100, "%02d", pt, ptlim);
							pt = strftime_add(t.ToString("yyyy").Substring(0, 2), pt, ptlim);
							continue;

						case 'c': // Abbreviated date/time representation (e.g. Thu Aug 23 14:55:02 2001)
							pt = strftime_fmt("%a %b %e %H:%M:%S %Y", t, pt, ptlim);
							continue;

						case 'D': // Short MM/DD/YY date
							pt = strftime_fmt("%m/%d/%y", t, pt, ptlim);
							continue;

						case 'd': // Day of the month, zero-padded (01-31)
							//pt = _conv(t->tm_mday, "%02d", pt, ptlim);
							pt = strftime_add(t.ToString("dd"), pt, ptlim);
							continue;

						case 'e': // Day of the month, space-padded ( 1-31)
							//pt = _conv(t->tm_mday, "%2d", pt, ptlim);
							pt = strftime_add(t.Day.ToString().PadLeft(2, ' '), pt, ptlim);
							continue;

						case 'F': // Short YYYY-MM-DD date
							pt = strftime_fmt("%Y-%m-%d", t, pt, ptlim);
							continue;

						case 'H': // Hour in 24h format (00-23)
							//pt = _conv(t->tm_hour, "%02d", pt, ptlim);
							pt = strftime_add(t.ToString("HH"), pt, ptlim);
							continue;

						case 'I': // Hour in 12h format (01-12)
							//pt = _conv((t->tm_hour % 12) ? (t->tm_hour % 12) : 12, "%02d", pt, ptlim);
							pt = strftime_add(t.ToString("hh"), pt, ptlim);
							continue;

						case 'j': // Day of the year (001-366)
							pt = strftime_add(t.DayOfYear.ToString().PadLeft(3, ' '), pt, ptlim);
							continue;

						case 'k': // (Non-standard) // Hours in 24h format, space-padded ( 1-23)
							//pt = _conv(t->tm_hour, "%2d", pt, ptlim);
							pt = strftime_add(t.ToString("%H").PadLeft(2, ' '), pt, ptlim);
							continue;

						case 'l': // (Non-standard) // Hours in 12h format, space-padded ( 1-12)
							//pt = _conv((t->tm_hour % 12) ? (t->tm_hour % 12) : 12, "%2d", pt, ptlim);
							pt = strftime_add(t.ToString("%h").PadLeft(2, ' '), pt, ptlim);
							continue;

						case 'M': // Minute (00-59)
							//pt = _conv(t->tm_min, "%02d", pt, ptlim);
							pt = strftime_add(t.ToString("mm"), pt, ptlim);
							continue;

						case 'm': // Month as a decimal number (01-12)
							//pt = _conv(t->tm_mon + 1, "%02d", pt, ptlim);
							pt = strftime_add(t.ToString("MM"), pt, ptlim);
							continue;

						case 'n': // New-line character.
							pt = strftime_add(Environment.NewLine, pt, ptlim);
							continue;

						case 'p': // AM or PM designation (locale dependent).
							//pt = _add((t->tm_hour >= 12) ? "pm" : "am", pt, ptlim);
							pt = strftime_add(t.ToString("tt"), pt, ptlim);
							continue;

						case 'R': // 24-hour HH:MM time, equivalent to %H:%M
							pt = strftime_fmt("%H:%M", t, pt, ptlim);
							continue;

						case 'r': // 12-hour clock time (locale dependent).
							pt = strftime_fmt("%I:%M:%S %p", t, pt, ptlim);
							continue;

						case 'S': // Second ((00-59)
							//pt = _conv(t->tm_sec, "%02d", pt, ptlim);
							pt = strftime_add(t.ToString("ss"), pt, ptlim);
							continue;

						case 'T': // ISO 8601 time format (HH:MM:SS), equivalent to %H:%M:%S
							pt = strftime_fmt("%H:%M:%S", t, pt, ptlim);
							continue;

						case 't': // Horizontal-tab character
							pt = strftime_add("\t", pt, ptlim);
							continue;

						case 'U': // Week number with the first Sunday as the first day of week one (00-53)
							//pt = _conv((t->tm_yday + 7 - t->tm_wday) / 7, "%02d", pt, ptlim);
							pt = strftime_add(System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(t, System.Globalization.CalendarWeekRule.FirstFullWeek, DayOfWeek.Sunday).ToString(), pt, ptlim);
							continue;

						case 'u': // ISO 8601 weekday as number with Monday as 1 (1-7) (locale independant).
							//pt = _conv((t->tm_wday == 0) ? 7 : t->tm_wday, "%d", pt, ptlim);
							pt = strftime_add(t.DayOfWeek == DayOfWeek.Sunday ? "7" : ((int)t.DayOfWeek).ToString(), pt, ptlim);
							continue;

						case 'G':   // ISO 8601 year (four digits)
						case 'g':  // ISO 8601 year (two digits)
						case 'V':   // ISO 8601 week number
							// See http://stackoverflow.com/questions/11154673/get-the-correct-week-number-of-a-given-date
							DateTime isoTime = t;
							DayOfWeek day = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(isoTime);
							if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
							{
								isoTime = isoTime.AddDays(3);
							}

							if (format[0] == 'V') // ISO 8601 week number
							{
								int isoWeek = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(isoTime, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
								pt = strftime_add(isoWeek.ToString(), pt, ptlim);
							}
							else
							{
								string isoYear = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetYear(isoTime).ToString(); // ISO 8601 year (four digits)

								if (format[0] == 'g') // ISO 8601 year (two digits)
								{
									isoYear = isoYear.Substring(isoYear.Length - 2, 2);
								}
								pt = strftime_add(isoYear, pt, ptlim);
							}

							continue;

						case 'W': // Week number with the first Monday as the first day of week one (00-53)
							//pt = _conv((t->tm_yday + 7 - (t->tm_wday ? (t->tm_wday - 1) : 6)) / 7, "%02d", pt, ptlim);
							pt = strftime_add(System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(t, System.Globalization.CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday).ToString(), pt, ptlim);
							continue;

						case 'w': // Weekday as a decimal number with Sunday as 0 (0-6)
							//pt = _conv(t->tm_wday, "%d", pt, ptlim);
							pt = strftime_add(((int)t.DayOfWeek).ToString(), pt, ptlim);
							continue;

						case 'X': // Long time representation (locale dependent)
							//pt = _fmt("%H:%M:%S", t, pt, ptlim); // fails to comply with spec!
							pt = strftime_add(t.ToString("%T"), pt, ptlim);
							continue;

						case 'x': // Short date representation (locale dependent)
							//pt = _fmt("%m/%d/%y", t, pt, ptlim); // fails to comply with spec!
							pt = strftime_add(t.ToString("%d"), pt, ptlim);
							continue;

						case 'y': // Last two digits of year (00-99)
							//pt = _conv((t->tm_year + TM_YEAR_BASE) % 100, "%02d", pt, ptlim);
							pt = strftime_add(t.ToString("yy"), pt, ptlim);
							continue;

						case 'Y': // Full year (all digits)
							//pt = _conv(t->tm_year + TM_YEAR_BASE, "%04d", pt, ptlim);
							pt = strftime_add(t.Year.ToString(), pt, ptlim);
							continue;

						case 'Z': // Timezone name or abbreviation (locale dependent) or nothing if unavailable (e.g. CDT)
							pt = strftime_add(TimeZoneInfo.Local.StandardName, pt, ptlim);
							continue;

						case 'z': // ISO 8601 offset from UTC in timezone (+/-hhmm), or nothing if unavailable
							TimeSpan ts = TimeZoneInfo.Local.GetUtcOffset(t);
							string offset = (ts.Ticks < 0 ? "-" : "+") + ts.TotalHours.ToString("#00") + ts.Minutes.ToString("00");
							pt = strftime_add(offset, pt, ptlim);
							continue;

						case '%': // Add '%'
							pt = strftime_add("%", pt, ptlim);
							continue;

						default:
							break;
					}
				}

				if (pt == ptlim) break;

				pt[0] = format[0];
				pt.inc();
			}

			return pt;
		}

		private static CharPtr strftime_add(CharPtr str, CharPtr pt, CharPtr ptlim)
		{
			pt[0] = str[0];
			str = str.next();

			while (pt < ptlim && pt[0] != 0)
			{
				pt.inc();

				pt[0] = str[0];
				str = str.next();
			}
			return pt;
		} 
		#endregion

		private static int os_time (lua_State L) {
		  DateTime t;
		  if (lua_isnoneornil(L, 1))  /* called without args? */
			t = DateTime.Now;  /* get current time */
		  else {
			luaL_checktype(L, 1, LUA_TTABLE);
			lua_settop(L, 1);  /* make sure table is at the top */
			int sec = getfield(L, "sec", 0);
			int min = getfield(L, "min", 0);
			int hour = getfield(L, "hour", 12);
			int day = getfield(L, "day", -1);
			int month = getfield(L, "month", -1) - 1;
			int year = getfield(L, "year", -1) - 1900;
			/*int isdst = */getboolfield(L, "isdst");	// todo: implement this - mjf
			t = new DateTime(year, month, day, hour, min, sec);
		  }
		  lua_pushnumber(L, t.Ticks);
		  return 1;
		}


		private static int os_difftime (lua_State L) {
		  long ticks = (long)luaL_checknumber(L, 1) - (long)luaL_optnumber(L, 2, 0);
		  lua_pushnumber(L, ticks/TimeSpan.TicksPerSecond);
		  return 1;
		}

		/* }====================================================== */

		// locale not supported yet
		private static int os_setlocale (lua_State L) {		  
		  /*
		  static string[] cat = {LC_ALL, LC_COLLATE, LC_CTYPE, LC_MONETARY,
							  LC_NUMERIC, LC_TIME};
		  static string[] catnames[] = {"all", "collate", "ctype", "monetary",
			 "numeric", "time", null};
		  CharPtr l = luaL_optstring(L, 1, null);
		  int op = luaL_checkoption(L, 2, "all", catnames);
		  lua_pushstring(L, setlocale(cat[op], l));
		  */
		  CharPtr l = luaL_optstring(L, 1, null);
		  lua_pushstring(L, "C");
		  return (l.ToString() == "C") ? 1 : 0;
		}


		private static int os_exit (lua_State L) {
#if XBOX
			luaL_error(L, "os_exit not supported on XBox360");
#else
#if SILVERLIGHT
            throw new SystemException();
#else
			Environment.Exit(EXIT_SUCCESS);
#endif
#endif
			return 0;
		}

		private readonly static luaL_Reg[] syslib = {
		  new luaL_Reg("clock",     os_clock),
		  new luaL_Reg("date",      os_date),
		  new luaL_Reg("difftime",  os_difftime),
		  new luaL_Reg("execute",   os_execute),
		  new luaL_Reg("exit",      os_exit),
		  new luaL_Reg("getenv",    os_getenv),
		  new luaL_Reg("remove",    os_remove),
		  new luaL_Reg("rename",    os_rename),
		  new luaL_Reg("setlocale", os_setlocale),
		  new luaL_Reg("time",      os_time),
		  new luaL_Reg("tmpname",   os_tmpname),
		  new luaL_Reg(null, null)
		};

		/* }====================================================== */



		public static int luaopen_os (lua_State L) {
		  luaL_register(L, LUA_OSLIBNAME, syslib);
		  return 1;
		}

	}
}
