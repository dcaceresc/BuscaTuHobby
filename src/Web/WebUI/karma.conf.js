module.exports = function (config) {
  config.set({
    basePath: '',
    frameworks: ['jasmine', '@angular-devkit/build-angular'],
    plugins: [
      require('karma-jasmine'),
      require('karma-chrome-launcher'),
      require('karma-jasmine-html-reporter'),
      require('karma-coverage'),
      
    ],
    client: {
      jasmine: {
        // aquí puedes añadir opciones de configuración de Jasmine
        // las opciones posibles están en https://jasmine.github.io/api/edge/Configuration.html
        // por ejemplo, puedes desactivar la ejecución aleatoria con `random: false`
        // o fijar una semilla concreta con `seed: 4321`
      },
      clearContext: false // deja visible en el navegador la salida de Jasmine Spec Runner
    },
    jasmineHtmlReporter: {
      suppressAll: true // elimina las trazas duplicadas
    },
    coverageReporter: {
      dir: require('path').join(__dirname, './coverage/'),
      subdir: '.',
      reporters: [
        { type: 'html' },
        { type: 'text-summary' }
      ]
    },
    reporters: ['progress', 'kjhtml'],
    port: 9876,
    colors: true,
    logLevel: config.LOG_INFO,
    autoWatch: true,
    browsers: ['Chrome'],
    singleRun: false,
    restartOnFileChange: true,
    listenAddress: 'localhost',
    hostname: 'localhost'
  });
};

