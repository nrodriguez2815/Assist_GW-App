using System;

namespace Assist_GW.BLL
{
    /// <summary>
    /// Esta clase se encarga de loguear la información recibida.
    /// </summary>
    public static class Feedback
    {
        ///<summary>Graba según corresponda la info que recibe por parametros.</summary>
        ///<return>Devuelve una excepción en caso de error.</return>
        ///<param name="text">Mensaje que se desea guardar/mostrar</param>
        ///<param name="line">Deja un reglón antes de mostrar el nuevo mensaje</param>
        ///<param name="type">1:Actividad, 2:Error, 3:Trama</param>
        ///<remarks>
        /// "Cache.Config.ActivityLog" = True:Graba toda la info, False:Graba solo los Errores.
        /// "Cache.Config.LogTypeId" = 1:Graba en Base, 2:Graba en Archivo, 3:Graba en ambos.
        /// </remarks>
        public static void Log(string text, bool line, int type)
        {
            try
            {
                DateTime dateLog = DateTime.Now;
                int instance = DTO.Properties.Settings.Default.InstanceId;

                /** 
                 * <summary>En caso de error en el inicio de la aplicación</summary>
                */
                if (Cache.Config == null) 
                {
                    Cache.Config = new DTO.Configuration();
                    Cache.Config.LogTypeId = 2;
                    Cache.Config.ActivityLog = true;
                    Cache.Config.LogPath = DTO.Properties.Settings.Default.DefaultLog;
                }             

                Console.WriteLine(dateLog.ToString("dd/MM/yyyy HH:mm:ss.fff") + " " + text);

                if (line)
                    Console.WriteLine(string.Empty);

                if(Cache.Config.ActivityLog && type < 4)
                {
                    switch (Cache.Config.LogTypeId)
                    {
                        case 1:
                            {
                                    DAL.LogInDB.InsertFeedback(instance, text, type, Cache.Config.InstanceName);
                            }
                            break;

                        case 2:
                            {
                                    DAL.LogInFile.InsertFeedback(instance, text, Cache.Config.LogPath);
                            }
                            break;

                        case 3:
                            {
                                    DAL.LogInDB.InsertFeedback(instance, text, type, Cache.Config.InstanceName);
                                    DAL.LogInFile.InsertFeedback(instance, text, Cache.Config.LogPath);
                            }
                            break;
                    }
                }
                else if (type == 2)
                {
                    DAL.LogInFile.InsertFeedback(instance, text, Cache.Config.LogPath);
                }                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
