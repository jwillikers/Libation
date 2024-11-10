# Libation Flatpak

The instructions here describe how to build and update the Libation Flatpak.

## Build

Get the source code.

    git clone https://github.com/rmcrackan/Libation.git
    cd Libation

Add the Flathub repository.

    flatpak remote-add --user --if-not-exists flathub https://dl.flathub.org/repo/flathub.flatpakrepo

Install Flatpak Builder.

    sudo apt install flatpak-builder

Build the Flatpak.

    flatpak-builder --user --install --install-deps-from=flathub --force-clean --repo=repo build-dir com.getlibation.Libation.yaml

Run the Flatpak.

    flatpak run com.getlibation.Libation

## Update

git clone https://github.com/rmcrackan/Libation.git
cd Libation

wget https://raw.githubusercontent.com/flatpak/flatpak-builder-tools/master/dotnet/flatpak-dotnet-generator.py

flatpak --user install flathub org.freedesktop.Sdk//24.08 org.freedesktop.Sdk.Extension.dotnet8//24.08

python flatpak-dotnet-generator.py --runtime linux-x64 --freedesktop 24.08 --dotnet 8 nuget-sources-x86_64.json Source/LibationAvalonia/LibationAvalonia.csproj
python flatpak-dotnet-generator.py --runtime linux-arm64 --freedesktop 24.08 --dotnet 8 nuget-sources-aarch64.json Source/LibationAvalonia/LibationAvalonia.csproj

--runtime linux-x64
--runtime linux-arm64

A list of package source archives and checksums are generated for *all* dependencies.
Ideally, these dependencies will be updated whenever the `package-lock.json` file changes to keep the Flatpak version using the same packages.
The Flatpak Node Generator is used to generate this list of packages from the `package-lock.json` file, which is included directly in the Flatpak manifest.
To generate an updated version of this file, `generated-sources.json`, follow the instructions here.

In the Flatpak manifest, be sure to update the node runtime to match the major version that is used by Stretchly.
This can be found in the `package.json` file for Stretchly.

Make sure to install npm.
You'll want to use the same version of nodejs as the node runtime for the Flatpak.

    sudo apt install jq npm pipx python3-aiohttp

Install yq.

    pipx install yq

Install the Flatpak Node Generator Python utility with `pipx`.

    pip install flatpak-builder-tools/node --runtime org.freedesktop.Platform --dotnet 8 nuget-sources.json

Ensure the Flatpak Node Generator and yq are in your `PATH`.

    pipx ensurepath

Fetch the Stretchly source code.

    git clone https://github.com/hovancik/stretchly.git

Checkout the appropriate tag.

    git -C stretchly switch -d $(yq -r '.modules[1].sources[0].commit' net.hovancik.Stretchly.yaml)

Run the script against the `package-lock.json` file as shown here.

    flatpak-node-generator npm stretchly/package-lock.json --electron-node-headers
