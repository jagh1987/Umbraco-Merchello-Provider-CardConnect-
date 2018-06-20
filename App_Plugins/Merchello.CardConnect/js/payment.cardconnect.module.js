(function () {
    angular.module('merchello.providers.cardconnect', [
        'merchello.models',
            'merchello.services',
            'merchello.providers.models',
            'merchello.providers.directives',
            'merchello.providers.resources',
            'merchello.providers.cardconnect.models',
            'merchello.providers.cardconnect.resources',
            'merchello.providers.cardconnect.controllers'
    ]);
    angular.module('merchello.providers.cardconnect.models', []);
    angular.module('merchello.providers.cardconnect.resources', ['merchello.providers.models']);
    angular.module('merchello.providers.cardconnect.controllers', ['merchello.providers.cardconnect.resources']);
    
    //Dependencies
    angular.module('merchello.plugins').requires.push('merchello.providers.cardconnect');
}());
