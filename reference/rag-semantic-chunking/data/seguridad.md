# Identidad y seguridad de Aurora

Esta política ficticia establece controles para cuentas y equipos. La autenticación identifica a la persona; la autorización limita qué operaciones puede realizar. Una contraseña correcta no concede permisos para todos los servicios. Los accesos se aprueban según el rol y se revisan periódicamente.

## Activación y MFA

La invitación inicial llega al correo personal validado por Recursos Humanos y vence a las veinticuatro horas. Al activarla se crea una contraseña individual. Antes de habilitar el repositorio, la persona registra un segundo factor en el portal Puerta. Las cuentas de equipo compartidas están prohibidas.

Los métodos aceptados son una llave física o una aplicación autenticadora. Los códigos de recuperación deben guardarse fuera de la notebook corporativa. El alta del segundo factor se comprueba durante el ticket ACC-14; la persona mentora puede acompañar, pero no recibir los códigos.

## Pérdida de dispositivo

Si se pierde el teléfono usado para MFA, se abre el ticket SEG-8 desde la mesa de ayuda. La mesa verifica la identidad mediante una llamada al contacto registrado y revoca el factor perdido. El plazo objetivo de atención es de cuatro horas hábiles; no es una garantía de recuperación inmediata.

Hasta completar la verificación no se desactiva MFA por correo informal. Si también se perdió la notebook, soporte bloquea su sesión y registra el número de inventario. La nueva inscripción del factor se realiza con un enlace temporal de un solo uso.

## Información sensible

Los ejemplos y pruebas usan datos sintéticos. No se copian documentos de clientes al entorno taller ni se incluyen claves en commits, prompts o capturas de pantalla. Un secreto expuesto debe revocarse aunque luego se borre el archivo; eliminarlo del último commit no prueba que no haya sido utilizado.

Checklist antes de compartir un diagnóstico:

- Quitar tokens, cookies y credenciales de logs.
- Sustituir identificadores personales por valores ficticios.
- Mantener el código de error y la marca temporal necesarios para investigar.
- Compartir el diagnóstico por el canal interno autorizado.

## Revisión de permisos

El responsable del servicio revisa los permisos cada treinta días. Los accesos de producción requieren justificación adicional y vencimiento explícito. El ingreso al repositorio durante la incorporación no habilita desplegar. Las excepciones se registran con responsable y fecha de revisión.

La baja de una persona revoca sesiones, credenciales y permisos del mismo día. Los archivos de trabajo se transfieren mediante el procedimiento administrativo; nunca se conserva una cuenta activa sólo para consultar sus documentos.
