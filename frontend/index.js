document.addEventListener('DOMContentLoaded', function() {

    document.getElementById('imageForm').addEventListener('submit', function (e) {
        e.preventDefault();

        const file = document.getElementById('archivo').files[0]
        let image = {
            imageBase64: null,
        }
        const lector = new FileReader();
        lector.onload = function (event) {
            const auxBase64= event.target.result;
            image.imageBase64 = auxBase64.split(',')[1];
            const url = 'https://localhost:7252/Textract';
            const opciones = {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json', // Tipo de contenido del cuerpo
                },
                body: JSON.stringify(image),
            };

            fetch(url, opciones)
                .then(response => {
                    if (response.ok) {
                        const textAreaResult = document.getElementById('result');
                        const spanResult = document.getElementById('confidence');
                        const responseElement = document.getElementById('response');
                        responseElement.style.display = 'block';
                        response.json().then((result)=>{
                            textAreaResult.value = result.prescriptionText;
                            const confidenceRounded = result.confidence.toFixed(2);
                            spanResult.innerHTML = confidenceRounded;
                        });
                    } else {
                        throw new Error('Error en la solicitud POST');
                    }
                })
                .then(data => {
                    console.log(data);
                })
                .catch(error => {
                    console.error('Error en la solicitud:', error);
                });
        };
        lector.readAsDataURL(file);

    });

});