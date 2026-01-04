// Swagger UI Authorization Field Injector
// This script adds an Authorization header input field to all endpoints

(function() {
    // Wait for Swagger UI to load
    const checkSwaggerLoaded = setInterval(() => {
        const tryOutButtons = document.querySelectorAll('.try-out__btn');
        
        if (tryOutButtons.length > 0) {
            clearInterval(checkSwaggerLoaded);
  console.log('? Swagger loaded. Injecting Authorization fields...');
   injectAuthFields();
        }
    }, 500);

    function injectAuthFields() {
        // Monitor for "Try it out" button clicks
    document.addEventListener('click', function(e) {
            if (e.target.classList.contains('try-out__btn') ||
   e.target.closest('.try-out__btn')) {
                
    setTimeout(() => {
         addAuthFieldToCurrentEndpoint(e.target);
      }, 100);
          }
        });
    }

    function addAuthFieldToCurrentEndpoint(button) {
   // Find the parent operation block
        const operationBlock = button.closest('.opblock');
     if (!operationBlock) return;

        // Check if Authorization field already exists
        if (operationBlock.querySelector('.auth-field-injected')) return;

        // Find the parameters section
       const parametersSection = operationBlock.querySelector('.parameters');
        if (!parametersSection) return;

        // Create Authorization parameter row
        const authRow = document.createElement('tr');
       authRow.className = 'parameters auth-field-injected';
 authRow.innerHTML = `
<td class="parameters-col_name">
    <div class="parameter__name">Authorization</div>
         <div class="parameter__type">string</div>
          <div class="parameter__in">(header)</div>
   </td>
    <td class="parameters-col_description">
    <div class="renderedMarkdown">
                    <p>JWT Authorization token. Enter your full Bearer token here.</p>
      <p><strong>Example:</strong> <code>Bearer eyJhbGciOiJIUzI1NiIsInR5...</code></p>
     </div>
    <input 
    class="auth-token-input" 
       type="text"
              placeholder="Bearer your_token_here"
         style="width: 100%; padding: 8px; margin-top: 8px; border: 1px solid #ccc; border-radius: 4px; font-family: monospace;"
    />
            </td>
  `;

        // Find the table body and add our row
        const tbody = parametersSection.querySelector('tbody');
        if (tbody) {
            tbody.appendChild(authRow);
            console.log('? Authorization field added');

           // Intercept the Execute button
 setupExecuteInterceptor(operationBlock);
   }
    }

    function setupExecuteInterceptor(operationBlock) {
        const executeButton = operationBlock.querySelector('.execute');
        if (!executeButton) return;

     // Store the original onclick
        const originalOnClick = executeButton.onclick;

        executeButton.addEventListener('click', function(e) {
// Get the auth token input value
  const authInput = operationBlock.querySelector('.auth-token-input');
      if (authInput && authInput.value.trim()) {
    // Inject the Authorization header into the request
       injectAuthHeader(authInput.value.trim());
   }
      }, true);
    }

    function injectAuthHeader(token) {
        // Intercept fetch to add Authorization header
        const originalFetch = window.fetch;
     
    window.fetch = function(...args) {
            if (args[1]) {
                args[1].headers = args[1].headers || {};
      // Only add if token is provided and not already set
                if (token && !args[1].headers['Authorization']) {
         args[1].headers['Authorization'] = token.startsWith('Bearer ') ? token : 'Bearer ' + token;
         console.log('? Authorization header added to request');
         }
     }
            
    // Restore original fetch after this call
setTimeout(() => {
    window.fetch = originalFetch;
      }, 100);
      
     return originalFetch.apply(this, args);
        };
    }

    console.log('?? Swagger Authorization Injector loaded');
})();
