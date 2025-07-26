# Stage 1: Build the frontend using Node
FROM node:22-alpine AS frontend-builder

WORKDIR /app

# Install and build
COPY package*.json ./
COPY ./src ./src
COPY ./public ./public

RUN npm install && npm run build

# Stage 2: Run Python backend & serve frontend
FROM python:3.11-alpine

WORKDIR /app

# Install Python dependencies
COPY ./requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy backend code
COPY ./api ./api

# Copy frontend build output
COPY --from=frontend-builder /app/build ./build

# EXPOSE port for backend (adjust if needed)
EXPOSE 3000

# Start your backend (e.g., uvicorn for FastAPI or Flask app)
# Update this command based on your backend framework and structure
CMD ["uvicorn", "api.main:app", "--host", "0.0.0.0", "--port", "3000"]