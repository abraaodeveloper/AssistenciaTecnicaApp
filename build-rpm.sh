#!/bin/bash

# Versão da aplicação
VERSION="1.0.0"

# Criar diretório de build
mkdir -p ~/rpmbuild/{BUILD,RPMS,SOURCES,SPECS,SRPMS}

# Publicar a aplicação
dotnet publish -c Release -r linux-x64 --self-contained false

# Criar diretório temporário para os arquivos
mkdir -p ~/rpmbuild/SOURCES/publish/
mkdir -p ~/rpmbuild/SOURCES/Assets/

# Copiar os arquivos necessários
cp -r bin/Release/net9.0/linux-x64/publish/* ~/rpmbuild/SOURCES/publish/
cp Assets/logo.svg ~/rpmbuild/SOURCES/Assets/logo.png  # Convertendo SVG para PNG
cp AssistenciaTecnicaApp.spec ~/rpmbuild/SPECS/

# Converter SVG para PNG (requer imagemagick)
convert -background none -size 256x256 Assets/logo.svg ~/rpmbuild/SOURCES/Assets/logo.png

# Construir o RPM
rpmbuild -ba ~/rpmbuild/SPECS/AssistenciaTecnicaApp.spec

# O RPM estará em ~/rpmbuild/RPMS/x86_64/ 