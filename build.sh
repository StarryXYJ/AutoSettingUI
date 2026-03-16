#!/bin/bash

# AutoSettingUI Build and Pack Script
# Usage: ./build.sh [options]
#
# Options:
#   -c, --configuration <config>  Build configuration (Debug/Release). Default: Release
#   -v, --version <version>       Override package version (e.g., 1.0.1)
#   -s, --skip-build              Skip build step, only pack
#   -h, --help                    Show this help message

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_DIR="$SCRIPT_DIR/artifacts"
CONFIGURATION="Release"
VERSION=""
SKIP_BUILD=false

PACKAGES=(
    "AutoSettingUI.Core:src/AutoSettingUI.Core/AutoSettingUI.Core.csproj"
    "AutoSettingUI.Generator:src/AutoSettingUI.Generator/AutoSettingUI.Generator.csproj"
    "AutoSettingUI.Extension.Shared:AutoSettingUI.Extension.Shared/AutoSettingUI.Extension.Shared.csproj"
    "AutoSettingUI.Avalonia:src/Extensions/AutoSettingUI.Avalonia/AutoSettingUI.Avalonia.csproj"
    "AutoSettingUI.Ursa:src/Extensions/AutoSettingUI.Ursa/AutoSettingUI.Ursa.csproj"
    "AutoSettingUI.WPF:src/Extensions/AutoSettingUI.WPF/AutoSettingUI.WPF.csproj"
)

show_help() {
    echo "AutoSettingUI Build Script"
    echo ""
    echo "Usage: ./build.sh [options]"
    echo ""
    echo "Options:"
    echo "  -c, --configuration <config>  Build configuration (Debug/Release). Default: Release"
    echo "  -v, --version <version>       Override package version (e.g., 1.0.1)"
    echo "  -s, --skip-build              Skip build step, only pack"
    echo "  -h, --help                    Show this help message"
    echo ""
    echo "Examples:"
    echo "  ./build.sh"
    echo "  ./build.sh -c Debug"
    echo "  ./build.sh -v 1.2.0"
    echo "  ./build.sh -s -v 1.0.1-beta"
}

parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            -c|--configuration)
                CONFIGURATION="$2"
                shift 2
                ;;
            -v|--version)
                VERSION="$2"
                shift 2
                ;;
            -s|--skip-build)
                SKIP_BUILD=true
                shift
                ;;
            -h|--help)
                show_help
                exit 0
                ;;
            *)
                echo "Unknown option: $1"
                show_help
                exit 1
                ;;
        esac
    done
}

show_menu() {
    echo ""
    echo "========================================"
    echo "  AutoSettingUI Build & Pack Script"
    echo "========================================"
    echo ""
    echo "Available packages:"
    echo ""
    
    local i=1
    for pkg in "${PACKAGES[@]}"; do
        local name="${pkg%%:*}"
        echo "  [$i] $name"
        ((i++))
    done
    
    echo ""
    echo "  [A] Build ALL packages"
    echo "  [Q] Quit"
    echo ""
}

get_user_selection() {
    show_menu
    
    read -p "Enter your choice (1-${#PACKAGES[@]}, A, or Q): " selection
    
    case ${selection^^} in
        [1-${#PACKAGES[@]}])
            local index=$((selection - 1))
            echo "${PACKAGES[$index]}"
            ;;
        A)
            printf '%s\n' "${PACKAGES[@]}"
            ;;
        Q)
            echo "Exiting..."
            exit 0
            ;;
        *)
            echo "Invalid selection. Please try again."
            get_user_selection
            ;;
    esac
}

build_project() {
    local pkg="$1"
    local name="${pkg%%:*}"
    local project="${pkg#*:}"
    local project_path="$SCRIPT_DIR/$project"
    
    if [[ ! -f "$project_path" ]]; then
        echo "Project not found: $project_path"
        return 1
    fi
    
    echo "  Building $name..."
    
    local build_args=("build" "$project_path" "-c" "$CONFIGURATION")
    if [[ -n "$VERSION" ]]; then
        build_args+=("/p:Version=$VERSION")
    fi
    
    dotnet "${build_args[@]}"
}

pack_project() {
    local pkg="$1"
    local name="${pkg%%:*}"
    local project="${pkg#*:}"
    local project_path="$SCRIPT_DIR/$project"
    
    if [[ ! -f "$project_path" ]]; then
        return 1
    fi
    
    echo "  Packing $name..."
    
    local pack_args=("pack" "$project_path" "-c" "$CONFIGURATION" "-o" "$OUTPUT_DIR")
    if [[ -n "$VERSION" ]]; then
        pack_args+=("/p:Version=$VERSION")
    fi
    
    dotnet "${pack_args[@]}"
}

build_selected() {
    echo ""
    echo "Building projects..."
    echo "Configuration: $CONFIGURATION"
    
    for pkg in "$@"; do
        build_project "$pkg"
    done
    
    echo "Build completed successfully!"
}

pack_selected() {
    mkdir -p "$OUTPUT_DIR"
    rm -f "$OUTPUT_DIR"/*.nupkg 2>/dev/null || true
    
    echo ""
    echo "Packing NuGet packages..."
    echo "Output directory: $OUTPUT_DIR"
    
    for pkg in "$@"; do
        pack_project "$pkg"
    done
    
    echo ""
    echo "========================================"
    echo "  Pack completed successfully!"
    echo "========================================"
    echo ""
    echo "Packages created in: $OUTPUT_DIR"
    
    for file in "$OUTPUT_DIR"/*.nupkg; do
        echo "  - $(basename "$file")"
    done
}

main() {
    parse_args "$@"
    
    echo "AutoSettingUI Build Script"
    echo "Configuration: $CONFIGURATION"
    [[ -n "$VERSION" ]] && echo "Version override: $VERSION"
    
    mapfile -t selected < <(get_user_selection)
    
    if [[ "$SKIP_BUILD" != true ]]; then
        build_selected "${selected[@]}"
    fi
    
    pack_selected "${selected[@]}"
}

main "$@"
