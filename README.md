# AI Service Router

API Gateway que distribuye requests entre múltiples servicios de IA usando round-robin.

## Servicios soportados
- Groq (llama-3.3-70b-versatile)
- Cerebras (llama3.1-8b)

## Configuración

1. Clona el repositorio:
```bash
git clone <tu-repo>
cd AIServiceRouter
```

2. Copia el archivo de ejemplo y configura tus API keys:
```bash
cp appsettings.Development.json.example appsettings.Development.json
```

3. Edita `appsettings.Development.json` con tus API keys reales

4. Ejecuta el proyecto:
```bash
dotnet run
```

## Uso

**Endpoint:** `POST http://localhost:3000/chat`

**Body:**
```json
{
  "messages": [
    {
      "role": "user",
      "content": "Hello!"
    }
  ]
}
```

**Response:** Server-Sent Events (SSE) stream

## Ejemplo con cURL
```bash
curl -N -X POST http://localhost:3000/chat \
  -H "Content-Type: application/json" \
  -d '{
    "messages": [
      {"role": "user", "content": "Explain quantum computing in simple terms"}
    ]
  }'
```

## Arquitectura

El servicio implementa un load balancer round-robin que alterna entre los servicios configurados en cada request.

## Variables de entorno

- `PORT`: Puerto del servidor (default: 3000)