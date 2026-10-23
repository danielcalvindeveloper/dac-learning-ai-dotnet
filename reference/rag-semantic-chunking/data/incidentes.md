# Atención de incidentes de Aurora

Este procedimiento ficticio describe cómo atender interrupciones del servicio. Su objetivo es restaurar la operación y conservar información útil para aprender. Un incidente no es un pedido habitual de acceso ni una solicitud de vacaciones; esas gestiones usan circuitos separados.

## Clasificación inicial

Se clasifica como prioridad P1 una interrupción total del portal para todos los usuarios. P2 corresponde a una función importante degradada con alternativa disponible. P3 identifica fallas menores sin impacto inmediato. La prioridad puede cambiar cuando aparece nueva evidencia; no se asigna sólo por quién reporta.

Para P1 se contacta a la guardia mediante el canal Alerta Roja. El coordinador declara el incidente y abre la sala de trabajo. Para P2 se registra un ticket INC y se avisa al responsable del servicio. Un pedido individual de reinicio de MFA se deriva a SEG-8, salvo que sea parte de una falla general.

## Diagnóstico y comunicación

La primera actualización de P1 debe publicarse dentro de quince minutos de la declaración. Luego se comunica el estado cada treinta minutos mientras dure la interrupción. La actualización distingue hechos comprobados, hipótesis y próxima acción; no promete una hora de solución sin evidencia.

El diagnóstico inicial reúne estos datos:

- Hora del primer error y servicios afectados.
- Código de error y ejemplo de solicitud sin información sensible.
- Último despliegue conocido y cambios recientes de configuración.
- Estado de las dependencias y alcance confirmado por monitoreo.

## Recuperación

Si el problema apareció después de un despliegue, el responsable puede aprobar un rollback a la versión estable anterior. El coordinador registra la decisión y soporte verifica la recuperación con una consulta de salud y una operación funcional de prueba. Recuperar el health check no basta para cerrar el incidente.

Cuando el servicio vuelve a operar, se anuncia la restauración y se mantiene observación durante veinte minutos. Si reaparece el error se continúa el mismo incidente. El registro conserva los horarios de decisión, recuperación y cierre; no se reemplaza toda la cronología por un resumen redactado después.

## Aprendizaje posterior

Los incidentes P1 tienen una revisión sin culpabilización dentro de dos días hábiles. Se identifica el disparador, las condiciones que permitieron el impacto y las señales que faltaron. Cada acción de mejora tiene un responsable y una fecha; escribir una revisión no equivale a ejecutar esas acciones.

La revisión se guarda en el repositorio de operaciones con el código del incidente. Los secretos y datos personales se eliminan antes de compartir evidencia. Las métricas distinguen tiempo de detección y tiempo de recuperación para evitar ocultar demoras con un único promedio.
