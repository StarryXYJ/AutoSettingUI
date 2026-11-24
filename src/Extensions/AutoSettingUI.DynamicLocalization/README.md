# AutoSettingUI.DynamicLocalization

Optional adapter from DynamicLocalization's `ICultureService` to AutoSettingUI's
`ILocalizationService`. Construct `DynamicLocalizationAdapter` with your existing
culture service, assign it to the panel's `LocalizationService`, and dispose the
adapter when it is no longer used. The application retains ownership of the
culture service and controls language changes.

See the [project documentation](https://github.com/StarryXYJ/AutoSettingUI) for usage.
