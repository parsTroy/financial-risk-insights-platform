# Dockerfile for C++ Quantitative Engines
FROM ubuntu:20.04

# Set environment variables
ENV DEBIAN_FRONTEND=noninteractive
ENV TZ=UTC

# Install system dependencies
RUN apt-get update && apt-get install -y \
    build-essential \
    cmake \
    g++ \
    make \
    libssl-dev \
    libffi-dev \
    python3-dev \
    python3-pip \
    && rm -rf /var/lib/apt/lists/*

# Install Python dependencies
RUN pip3 install --no-cache-dir numpy scipy pandas psutil flask

# Set working directory
WORKDIR /app

# Copy source files
COPY . /app/

# Build C++ engines
RUN cmake . && make

# Create a simple HTTP server for the C++ engines
RUN echo '#!/bin/bash\n\
echo "C++ Quantitative Engines Service"\n\
echo "Port: $PORT"\n\
echo "Starting HTTP server..."\n\
python3 -c "\n\
import http.server\n\
import socketserver\n\
import os\n\
\n\
class QuantEnginesHandler(http.server.BaseHTTPRequestHandler):\n\
    def do_GET(self):\n\
        if self.path == \"/health\":\n\
            self.send_response(200)\n\
            self.send_header(\"Content-type\", \"application/json\")\n\
            self.end_headers()\n\
            self.wfile.write(b\'{\"status\": \"healthy\", \"service\": \"cpp-engines\"}\')\n\
        else:\n\
            self.send_response(404)\n\
            self.end_headers()\n\
    \n\
    def do_POST(self):\n\
        if self.path == \"/monte-carlo\":\n\
            self.send_response(200)\n\
            self.send_header(\"Content-type\", \"application/json\")\n\
            self.end_headers()\n\
            self.wfile.write(b\'{\"result\": \"C++ Monte Carlo engine response\"}\')\n\
        else:\n\
            self.send_response(404)\n\
            self.end_headers()\n\
\n\
port = int(os.environ.get(\"PORT\", 8080))\n\
with socketserver.TCPServer((\"\", port), QuantEnginesHandler) as httpd:\n\
    print(f\"C++ Engines server running on port {port}\")\n\
    httpd.serve_forever()\n\
"' > start_server.sh && chmod +x start_server.sh

# Expose port
EXPOSE 8080

# Start the server
CMD ["./start_server.sh"]
