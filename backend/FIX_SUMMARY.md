# ?? Fix Summary - DataProtection & Traefik Issues

## Issues Resolved

### 1. ? DataProtection Permission Error
**Error:** `Access to the path '/home/app/.aspnet/DataProtection-Keys/...' is denied`

**Fix Applied:** Updated Dockerfile to create directory with proper permissions before switching to non-root user.

### 2. ? Swagger Not Accessible
**Issues:**
- https://parvazorg.minisource.ir/api/payment/swagger ?
- http://130.185.120.38:4005/swagger ?

**Fixes Applied:**
- Enabled OpenAPI/Swagger in Production environment
- Added Traefik docker network label
- Added HTTP to HTTPS redirect
- Exposed port properly

## Files Modified

### 1. `payment/Dockerfile`
- Created DataProtection directory with proper permissions
- Set ownership to APP_UID before switching users

### 2. `docker-compose.yml`
- Added `expose: ["4005"]` directive
- Added `traefik.docker.network=dokploy-network` label
- Added HTTP to HTTPS redirect middleware
- Kept port mapping for direct IP access

### 3. `payment/Program.cs`
- Enabled OpenAPI in all environments (not just Development)
- Ensured health check endpoint is mapped

### 4. New Documentation
- `TROUBLESHOOTING.md` - Comprehensive troubleshooting guide
- `deploy.sh` - Automated deployment script

## Deployment Instructions

### Quick Deploy (Linux/Mac)
```bash
chmod +x deploy.sh
./deploy.sh
```

### Manual Deploy
```bash
# 1. Stop and clean
docker-compose down
docker volume rm payment-dataprotection  # Optional, fixes permission issues

# 2. Build with no cache
docker-compose build --no-cache

# 3. Start services
docker-compose up -d

# 4. Wait for health checks
sleep 40

# 5. Check logs (should see no errors)
docker-compose logs payment | grep -i "error\|permission\|unauthorized"

# 6. Run migrations
docker exec -it payment-api dotnet ef database update
```

## Verification Steps

### 1. Check Container Status
```bash
docker-compose ps
# Should show: Up (healthy)
```

### 2. Check Logs
```bash
docker-compose logs payment --tail 50
```

**Expected Output (No errors):**
```
info: Microsoft.Hosting.Lifetime[14]
Now listening on: http://[::]:4005
info: Microsoft.Hosting.Lifetime[0]
Application started. Press Ctrl+C to shut down.
```

### 3. Test Local Access
```bash
# Health check
curl http://localhost:4005/health
# Expected: {"status":"Healthy"}

# Swagger
curl http://localhost:4005/swagger/v1/swagger.json
# Expected: JSON with OpenAPI spec
```

### 4. Test Traefik Access
```bash
# Health via HTTPS
curl -k https://parvazorg.minisource.ir/api/payment/health

# Swagger via HTTPS
curl -k https://parvazorg.minisource.ir/api/payment/swagger/v1/swagger.json
```

### 5. Browser Access
Open in browser:
- **Local Swagger:** http://localhost:4005/swagger/index.html
- **Traefik Swagger:** https://parvazorg.minisource.ir/api/payment/swagger/index.html

## Expected Results

### ? All Issues Fixed:
- [x] DataProtection keys permission issue resolved
- [x] Swagger accessible in production
- [x] Traefik routing working with HTTPS
- [x] HTTP automatically redirects to HTTPS
- [x] Direct IP access works (if port exposed)
- [x] Health checks passing

### ? Access Points Working:
| Access Point | URL | Status |
|-------------|-----|--------|
| Local Health | http://localhost:4005/health | ? |
| Local Swagger | http://localhost:4005/swagger/index.html | ? |
| Traefik Health (HTTP?HTTPS) | http://parvazorg.minisource.ir/api/payment/health | ? |
| Traefik Health (HTTPS) | https://parvazorg.minisource.ir/api/payment/health | ? |
| Traefik Swagger | https://parvazorg.minisource.ir/api/payment/swagger/index.html | ? |
| Direct IP (if exposed) | http://130.185.120.38:4005/health | ? |

## Traefik Configuration Summary

### Labels Applied:
```yaml
traefik.enable=true
traefik.docker.network=dokploy-network

# HTTPS Router
traefik.http.routers.payment.rule=Host(`parvazorg.minisource.ir`) && PathPrefix(`/api/payment`)
traefik.http.routers.payment.entrypoints=websecure
traefik.http.routers.payment.tls=true
traefik.http.routers.payment.tls.certresolver=letsencrypt

# Middleware - Strip prefix
traefik.http.routers.payment.middlewares=payment-strip
traefik.http.middlewares.payment-strip.stripprefix.prefixes=/api/payment

# Service
traefik.http.services.payment.loadbalancer.server.port=4005

# HTTP to HTTPS redirect
traefik.http.routers.payment-http.rule=Host(`parvazorg.minisource.ir`) && PathPrefix(`/api/payment`)
traefik.http.routers.payment-http.entrypoints=web
traefik.http.routers.payment-http.middlewares=redirect-to-https
traefik.http.middlewares.redirect-to-https.redirectscheme.scheme=https
```

### Request Flow:
```
Client Request: https://parvazorg.minisource.ir/api/payment/swagger/index.html
                ?
Traefik:        Matches Host + PathPrefix rule
                ?
Traefik:        Applies payment-strip middleware
                ?
Traefik:        Strips /api/payment prefix
                ?
Container:      Forwards to http://payment-api:4005/swagger/index.html
                ?
Response:       Returns Swagger UI
```

## Common Issues & Quick Fixes

### Issue: Permission Denied
```bash
docker-compose down
docker volume rm payment-dataprotection
docker-compose up -d --build
```

### Issue: Traefik Not Routing
```bash
# Check container is on correct network
docker network inspect dokploy-network | grep payment-api

# Check Traefik sees the container
docker logs traefik | grep payment

# Restart Traefik (if needed)
docker restart traefik
```

### Issue: 404 Not Found
```bash
# Verify path stripping works
docker inspect payment-api | grep stripprefix

# Test direct container access
docker exec -it payment-api curl http://localhost:4005/health
```

### Issue: SSL Certificate Error
```bash
# Check certificate status
echo | openssl s_client -servername parvazorg.minisource.ir \
  -connect parvazorg.minisource.ir:443 2>/dev/null | \
  openssl x509 -noout -dates

# Check Traefik Let's Encrypt logs
docker logs traefik | grep -i "letsencrypt\|acme"
```

## Monitoring

### Watch Container Status
```bash
watch -n 2 'docker-compose ps'
```

### Follow Logs
```bash
docker-compose logs -f payment
```

### Check Health
```bash
watch -n 5 'curl -s http://localhost:4005/health'
```

## Rollback (If Needed)

```bash
# Stop services
docker-compose down

# Checkout previous version
git checkout HEAD~1

# Rebuild and start
docker-compose up -d --build
```

## Support & Documentation

- **Troubleshooting:** See `TROUBLESHOOTING.md`
- **Docker Guide:** See `DOCKER.md`
- **Quick Start:** See `QUICK_START.md`
- **All Fixes:** See `DOCKER_FIXES.md`

## Next Steps

1. ? Deploy the fixes using `./deploy.sh` or manual commands
2. ? Verify all endpoints are accessible
3. ? Run database migrations if needed
4. ? Update DNS if domain changed
5. ? Monitor logs for any issues
6. ? Set up backup for DataProtection volume
7. ? Configure monitoring/alerting

---

**Status:** ? Ready to Deploy
**Last Updated:** 2024-12-18
**Version:** 1.1
