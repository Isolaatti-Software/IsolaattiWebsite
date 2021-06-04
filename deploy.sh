# Detengo el servicio y elimino los dlls
echo "Deteniendo servicio"
ssh root@isolaatti.com "systemctl stop isolaatti-app && rm -r /var/www/isolaatti/publish"
# Copio los dlls
echo "Comenzando copia de los dlls al servidor"
scp -r ./bin/Release/netcoreapp3.1/publish root@isolaatti.com:/var/www/isolaatti
# Cambio permisos
echo "Cambiando propietario a todos los archivos a www-root"
ssh root@isolaatti.com "cd /var/www/isolaatti/publish/ && chown www-data *"
# Inicio servicio
echo "Iniciando servicio..."
ssh root@isolaatti.com "systemctl start isolaatti-app"