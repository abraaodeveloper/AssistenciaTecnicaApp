Name:           assistencia-tecnica-app
Version:        1.0.0
Release:        1%{?dist}
Summary:        Sistema de Assistência Técnica
License:        MIT
URL:            https://github.com/seu-usuario/AssistenciaTecnicaApp
BuildArch:      x86_64

# Dependências necessárias
Requires:       dotnet-runtime-9.0

%description
Sistema para gerenciamento de ordens de serviço de assistência técnica.

%prep
# Nada a preparar, já que vamos usar o binário publicado

%build
# Nada a construir aqui, vamos usar o binário publicado

%install
rm -rf $RPM_BUILD_ROOT
mkdir -p $RPM_BUILD_ROOT/usr/share/assistencia-tecnica
mkdir -p $RPM_BUILD_ROOT/usr/share/applications
mkdir -p $RPM_BUILD_ROOT/usr/share/icons/hicolor/256x256/apps

# Copiar os arquivos da aplicação
cp -r %{_sourcedir}/publish/* $RPM_BUILD_ROOT/usr/share/assistencia-tecnica/

# Criar o arquivo .desktop
cat > $RPM_BUILD_ROOT/usr/share/applications/assistencia-tecnica.desktop << EOF
[Desktop Entry]
Name=Assistência Técnica
Comment=Sistema de Assistência Técnica
Exec=/usr/share/assistencia-tecnica/AssistenciaTecnicaApp
Icon=assistencia-tecnica
Terminal=false
Type=Application
Categories=Office;
EOF

# Copiar o ícone
cp %{_sourcedir}/Assets/logo.png $RPM_BUILD_ROOT/usr/share/icons/hicolor/256x256/apps/assistencia-tecnica.png

%files
%{_datadir}/assistencia-tecnica
%{_datadir}/applications/assistencia-tecnica.desktop
%{_datadir}/icons/hicolor/256x256/apps/assistencia-tecnica.png

%post
# Atualizar cache de ícones
gtk-update-icon-cache -f -t /usr/share/icons/hicolor || :

%postun
# Atualizar cache de ícones na remoção
gtk-update-icon-cache -f -t /usr/share/icons/hicolor || : 